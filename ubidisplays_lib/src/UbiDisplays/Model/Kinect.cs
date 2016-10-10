using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using System.Windows;
using System.Windows.Media.Imaging;

using Microsoft.Kinect;

namespace UbiDisplays.Model
{

    /// <summary>
    /// Manages sensor activation, deactivation and processing requirements for the UbiDisplays toolkit.
    /// </summary>
    /// <remarks>In essence, this class is responsible for mapping the relevant data from the colour and 3D point graphs onto a depth map.  This class is threadsafe.</remarks>
    /// <author>John Hardy</author>
    /// <date>14th Nov 2012</date>
    public class KinectProcessing : IDisposable
    {
        /// <summary>
        /// How should we render to the calibration and display drawing aspects of the UI.
        /// </summary>
        public enum RenderStrategy
        {
            /// <summary>
            /// Render colour blended with additional information such as sensor errors and selected interaction points.
            /// </summary>
            ColourAndErrors,

            /// <summary>
            /// Render the scene using grayscale depth.
            /// </summary>
            GrayscaleDepth,

            /// <summary>
            /// Render the scene using rgb heatmap depth.
            /// </summary>
            RGBDepth,

            /// <summary>
            /// Render the scene using the depth of each pixel relative to a given point.
            /// </summary>
            RGBDepthRelative,
        }

        /// <summary>
        /// Is this sensor active and processing data.  Threadsafe.
        /// </summary>
        public bool IsActive
        {
            get
            {
                mControlMutex.WaitOne();
                bool bData = bActive;
                mControlMutex.ReleaseMutex();
                return bData;
            }
        }
        /// <summary>
        /// The internal variable for the Active property.
        /// </summary>
        private bool bActive = false;

        /// <summary>
        /// Get the current processing rate in frames per second.
        /// </summary>
        public int FPS { get { return _FPSCounter.FPS; } }
        /// <summary>
        /// The FPS counter.
        /// </summary>
        private Utilities.FPSCounter _FPSCounter = new Utilities.FPSCounter();

        /// <summary>
        /// Return a reference to the kinect sensor.  Threadsafe.
        /// </summary>
        public KinectSensor Sensor
        {
            get
            {
                mControlMutex.WaitOne();
                KinectSensor pSensor = this.pSensor;
                mControlMutex.ReleaseMutex();
                return pSensor;
            }
        }

        /// <summary>
        /// The internal variable for the Sensor property.
        /// </summary>
        private KinectSensor pSensor = null;

        /// <summary>
        /// A mutex which controls the use of options (i.e. IsActive).
        /// </summary>
        private Mutex mControlMutex = new Mutex();

        /// <summary>
        /// Control threadsafe access to the index buffers (i.e. depth to 3d and depth to colour).
        /// </summary>
        private Mutex mIndexLock = new Mutex();

        /// <summary>
        /// The thread which deals with our processing.
        /// </summary>
        private Thread pProcessingThread = null;

        #region Data buffers
        /// <summary>
        /// A back buffer (of sorts) for the pixel data which will be displayed on the UI.  One entry for each depth pixel.
        /// </summary>
        private Utilities.DoubleBuffer<byte[]> pInterfacePixels = null;

        /// <summary>
        /// A back buffer (of sorts) for the pixel data which will be displayed on the UI.  One entry for each depth pixel.
        /// </summary>
        private Utilities.DoubleBuffer<Utilities.Triple<int, SlimMath.Vector3[], int[]>> pPointCloud = null;

        /// <summary>
        /// A buffer which contains the pixel data for the colour frame.  One entry for each colour pixel.
        /// </summary>
        public byte[] tColourPixels = null;

        /// <summary>
        /// A buffer which contains the pixel data for the depth frame.  One entry for each depth pixel.
        /// </summary>
        public DepthImagePixel[] tDepthPixels = null;

        /// <summary>
        /// A buffer which contains the skeleton points as derived from each depth point.  One entry for each depth pixel.
        /// </summary>
        private SkeletonPoint[] tDepthToSkeleton = null;

        /// <summary>
        /// A buffer which contains the index buffer which maps depth points onto to colour points.  One entry for each depth pixel.
        /// </summary>
        private ColorImagePoint[] tDepthToColourIB = null;


        /// <summary>
        /// The width of the depth processing frame.
        /// </summary>
        private int iDepthWidth = 0;
        /// <summary>
        /// The height of the depth processing frame.
        /// </summary>
        private int iDepthHeight = 0;
        /// <summary>
        /// The width of the colour processing frame.
        /// </summary>
        private int iColourWidth = 0;
        /// <summary>
        /// The height of the colour processing frame.
        /// </summary>
        private int iColourHeight = 0;
        /// <summary>
        /// How many depth images fit in a colour image.
        /// </summary>
        private int iColourToDepthDivisor = 2;
        #endregion

        /// <summary>
        /// Get or set the render strategy for this processing class.
        /// </summary>
        public RenderStrategy ActiveRenderStrategy
        {
            get
            {
                mControlMutex.WaitOne();
                var eData =  this.eRenderStrategy;
                mControlMutex.ReleaseMutex();
                return eData;
            }
            set
            {
                mControlMutex.WaitOne();
                this.eRenderStrategy = value;
                mControlMutex.ReleaseMutex();
            }
        }
        /// <summary>
        /// The internal variable for the ActiveRenderStrategy property.
        /// </summary>
        private RenderStrategy eRenderStrategy = RenderStrategy.ColourAndErrors;

        /// <summary>
        /// A bit array which we want to use to quickly discard pixels.  True means accept pixel.
        /// </summary>
        public System.Collections.BitArray EnabledPixels
        {
            get
            {
                mControlMutex.WaitOne();
                var e = _EnabledPixels;
                mControlMutex.ReleaseMutex();
                return e;
            }
            set
            {
                mControlMutex.WaitOne();
                _EnabledPixels = value.Clone() as System.Collections.BitArray;
                mControlMutex.ReleaseMutex();
            }
        }
        /// <summary>
        /// The internal variable for the speedy discard of discarded pixel regions.
        /// </summary>
        private System.Collections.BitArray _EnabledPixels = null;


        /// <summary>
        /// A bit array which we want to use to quickly discard pixels.  True means accept pixel.
        /// </summary>
        public SlimMath.Plane CalibrationPlane
        {
            get
            {
                mControlMutex.WaitOne();
                var pPlane = _CalibrationPlane;
                mControlMutex.ReleaseMutex();
                return pPlane;
            }
            set
            {
                mControlMutex.WaitOne();
                _CalibrationPlane = value;
                mControlMutex.ReleaseMutex();
            }
        }
        /// <summary>
        /// The internal variable for the speedy discard of discarded pixel regions.
        /// </summary>
        private SlimMath.Plane _CalibrationPlane;

        /// <summary>
        /// An event which is rasied once we have finished processing a frame.
        /// </summary>
        /// <remarks>This is raised AFTER the internal double buffer is flipped.  This is also called from the Kinect thread.</remarks>
        public event Action<KinectProcessing> OnFrameReady;

        /// <summary>
        /// An event which is raised once the Kinect startup process has finished and it has streamed its first frame.
        /// </summary>
        public event Action<KinectProcessing> OnKinectStreamingStarted;
        private bool bStreamedFirstFrame = false;

        /// <summary>
        /// Ensure that everything is correctly removed and released.
        /// </summary>
        public void Dispose()
        {
            // Ensure all the threads are stopped.
            
            // Ensure the sensor is stopped.
            this.Stop();
        }

        /// <summary>
        /// Activate this sensor.
        /// </summary>
        /// <param name="pSensor">The sensor which we want to invoke processing on.</param>
        public void Start(KinectSensor pSensor, bool bUseThread = false)
        {
            // Acquire the control mutex.
            mControlMutex.WaitOne();

            // If we are already active, bail.
            if (bActive)
            {
                mControlMutex.ReleaseMutex();
                throw new ArgumentException("Cannot start processing on Kinect sensor when another one is already active.");
            }

            // Reset the streaming flag.
            bStreamedFirstFrame = false;

            // If we are using threading.
            if (bUseThread)
            {
                // Release the mutex (so we can re-enter).
                mControlMutex.ReleaseMutex();

                // Check for problems.
                if (pProcessingThread != null)
                    throw new Exception("Cannot start Kinect.  Thread is already active.");

                // Create a thread which will start the sensor.
                pProcessingThread = new Thread(() =>
                    {
                        this.Start(pSensor, false);
                        Console.WriteLine("Kinect Thread = " + pProcessingThread.Name);
                    });
                //pProcessingThread.Name = "Processing Thread";
                pProcessingThread.Start();
                return;
            }

            #region Sensor Setup and Buffer Creation
            // Set the reference on the sensor.
            this.pSensor = pSensor;
            this.pSensor.AllFramesReady += Handle_AllFramesReady;
            
            // Enable the streams we are interested in.
            pSensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
            pSensor.DepthStream.Enable(DepthImageFormat.Resolution320x240Fps30);
            
            // Work out the maximum amount of pixels we will need to process.
            var iMaxDepthPixelCount = pSensor.DepthStream.FrameWidth * pSensor.DepthStream.FrameHeight;

            // check out that near mode
            pSensor.DepthStream.Range = DepthRange.Near;

            // Create double buffers for the interface pixels and point cloud.
            pInterfacePixels = new Utilities.DoubleBuffer<byte[]>(new byte[iMaxDepthPixelCount * 4], new byte[iMaxDepthPixelCount * 4]);
            pPointCloud = new Utilities.DoubleBuffer<Utilities.Triple<int, SlimMath.Vector3[], int[]>>(
                new Utilities.Triple<int, SlimMath.Vector3[], int[]>(0, new SlimMath.Vector3[iMaxDepthPixelCount], new int[iMaxDepthPixelCount]),
                new Utilities.Triple<int, SlimMath.Vector3[], int[]>(0, new SlimMath.Vector3[iMaxDepthPixelCount], new int[iMaxDepthPixelCount])
                );

            // Create buffers (each is the size of the depth stream * 4bpp where necessary).
            tDepthPixels     = new DepthImagePixel[iMaxDepthPixelCount];
            tDepthToColourIB = new ColorImagePoint[iMaxDepthPixelCount];
            tDepthToSkeleton = new SkeletonPoint[iMaxDepthPixelCount];

            // Create a buffer for the colour video feed (the size of the colour stream * 4bpp).
            tColourPixels     = new byte[pSensor.ColorStream.FramePixelDataLength];

            // Setup stream properties.
            this.iDepthWidth  = pSensor.DepthStream.FrameWidth;
            this.iDepthHeight = pSensor.DepthStream.FrameHeight;
            this.iColourWidth  = pSensor.ColorStream.FrameWidth;
            this.iColourHeight = pSensor.ColorStream.FrameHeight;
            this.iColourToDepthDivisor = this.iColourWidth / this.iDepthWidth;

            // If we have not got an enabled pixel array, make one.
            if (this._EnabledPixels == null)
                this._EnabledPixels = new System.Collections.BitArray(iMaxDepthPixelCount, true);
            #endregion

            // Reset the FPS counter.
            _FPSCounter.Reset();

            // Start the sensor.
            this.pSensor.Start();

            // Set our flag to be active.
            bActive = true;

            // Release the mutex.
            mControlMutex.ReleaseMutex();
        }

        /// <summary>
        /// Deactivate this sensor and free up the memory.
        /// </summary>
        public void Stop()
        {
            // Acquire the control mutex.
            mControlMutex.WaitOne();

            // If we are already not active, bail.
            if (!bActive)
            {
                mControlMutex.ReleaseMutex();
                return;
            }

            // If we have a processing thread, shut it down.
            if (pProcessingThread != null)
            {
                if (!pProcessingThread.Join(10000))
                {
                    mControlMutex.ReleaseMutex();
                    Log.Write("Could not stop processing thread within 1 second.  Aborting.  You may need to restart the application.", "Application", Log.Type.AppError);
                    pProcessingThread.Abort();
                    mControlMutex.WaitOne();
                }
                pProcessingThread = null;
            }

            #region Sensor shutdown and buffer removal
            // Unhook the events.
            this.pSensor.AllFramesReady -= Handle_AllFramesReady;
            this.pSensor.Stop();

            // Remove the buffers.
            pInterfacePixels = null;
            pPointCloud = null;
            tDepthPixels = null;
            tDepthToColourIB = null;
            tDepthToSkeleton = null;
            tColourPixels = null;

            // Remove the reference to the sensor.
            this.pSensor = null;
            #endregion

            // Set our flag to be not active.
            bActive = false;

            // Release the mutex.
            mControlMutex.ReleaseMutex();
        }


        /// <summary>
        /// Process the new frame of data from the Kinect.
        /// </summary>
        /// <remarks>Don't put too much processing here if possible because we can't get another frame until this one has finished.</remarks>
        /// <param name="sender">The sensor which raised the event.</param>
        /// <param name="e">The event which contains the frame data.</param>
        private void Handle_AllFramesReady(object sender, AllFramesReadyEventArgs e)
        {
            //if (pProcessingThread != null)
            //    Console.WriteLine("Processing Thread = " + pProcessingThread.Name);

            #region Get control data.
            // Acquire the control mutex.
            mControlMutex.WaitOne();

            // If we are not active, bail.
            if ((!bActive) || (this.pSensor == null) || (!this.pSensor.IsRunning))
            {
                mControlMutex.ReleaseMutex();
                return;
            }

            // Copy out control values.
            var eRenderStrategy = this.eRenderStrategy;
            var tBitArray = this._EnabledPixels;
            var tRenderPlane = this._CalibrationPlane;
            // - max and min colour controls
            

            // Release the control mutex.
            mControlMutex.ReleaseMutex();
            #endregion
            
            // Lock access to the image buffer to write into.
            //pInterfacePixels.LockNext();
            //pPointCloud.LockNext();

            #region Depth Map Processing
            // Open the depth frame.
            using (DepthImageFrame pDepthFrame = e.OpenDepthImageFrame())
            {
                // If the frame is not present, bail.
                if (pDepthFrame != null)
                {
                    // Copy the depth data into a buffer.
                    pDepthFrame.CopyDepthImagePixelDataTo(tDepthPixels);

                    // Acquire access to the coordinate map lock.
                    mIndexLock.WaitOne();

                    // Compute the index buffer which will get us the depth->colour pixel mapping.
                    pSensor.CoordinateMapper.MapDepthFrameToColorFrame(pSensor.DepthStream.Format, tDepthPixels, pSensor.ColorStream.Format, tDepthToColourIB);

                    // Map each depth pixel to a 3D point.
                    pSensor.CoordinateMapper.MapDepthFrameToSkeletonFrame(pSensor.DepthStream.Format, tDepthPixels, tDepthToSkeleton);

                    // Copy the colour pixels - if we are using the colour render strategy.
                    if (eRenderStrategy == RenderStrategy.ColourAndErrors)
                    {
                        using (ColorImageFrame pColourFrame = e.OpenColorImageFrame())
                        {
                            // Block copy the colour data into our buffer so we can pull out what we need later.
                            if (pColourFrame != null)
                                pColourFrame.CopyPixelDataTo(tColourPixels);
                        }
                    }

                    // Release access.
                    mIndexLock.ReleaseMutex();

                    // Lock access to the image buffer to write into.
                    pInterfacePixels.LockNext();
                    pPointCloud.LockNext();

                    // Bring the interface pixel and point cloud array into scope.
                    var tInterfacePixels = pInterfacePixels.Next;
                    var iPointCloudCount = 0; // Store in pPointCloud.Next.First;
                    var tPointCloud = pPointCloud.Next.Second;
                    var tPointCloudPixelIndex = pPointCloud.Next.Third;

                    // For each pixel in the depth image.
                    DepthImagePixel pDepthPixel;
                    int iInterfacePtr = 0;
                    for (int i = 0, n = pDepthFrame.Width * pDepthFrame.Height; i < n; ++i)
                    {
                        // Address the depth pixel.
                        pDepthPixel = tDepthPixels[i];
                        iInterfacePtr = i * 4;

                        // If the depth is not known - skip.
                        if (!pDepthPixel.IsKnownDepth)
                        {
                            tInterfacePixels[iInterfacePtr + 0] = 0xFF;
                            tInterfacePixels[iInterfacePtr + 1] = 0xFF;
                            tInterfacePixels[iInterfacePtr + 2] = 0xFF;
                            continue;
                        }
                        //else if (pDepthPixel.Depth == pSensor.DepthStream.TooFarDepth)
                        //{
                        //}
                        //else if (pDepthPixel.Depth == pSensor.DepthStream.TooNearDepth)
                        //{
                        //}

                        // Is this pixel marked as ignored?
                        else if (!tBitArray.Get(i))
                        {
                            Utilities.ColourUtils.Greyscale(pDepthPixel.Depth, 0, 3000, ref tInterfacePixels[iInterfacePtr + 0], ref tInterfacePixels[iInterfacePtr + 1], ref tInterfacePixels[iInterfacePtr + 2]);
                            tInterfacePixels[iInterfacePtr + 2] = 0xCC; // Add a red-tint to the ignored greyscale regions.
                        }
                        

                        // This pixel is a valid, normal pixel.  Process with a render stratergy.
                        else
                        {
                            // Add the 3D pixel to the point cloud list.
                            tPointCloud[iPointCloudCount].X = tDepthToSkeleton[i].X;
                            tPointCloud[iPointCloudCount].Y = tDepthToSkeleton[i].Y;
                            tPointCloud[iPointCloudCount].Z = tDepthToSkeleton[i].Z;
                            tPointCloudPixelIndex[iPointCloudCount] = iInterfacePtr;
                            iPointCloudCount++;


                            // Select what we put in the image buffer based on the render stratergy.
                            switch (eRenderStrategy)
                            {
                                case RenderStrategy.ColourAndErrors:
                                    {
                                        ColorImagePoint pColourIndex = tDepthToColourIB[i];
                                        int iColorInDepthX = pColourIndex.X / this.iColourToDepthDivisor;
                                        int iColorInDepthY = pColourIndex.Y / this.iColourToDepthDivisor;
                                        if (iColorInDepthX > 0 && iColorInDepthX < iDepthWidth && iColorInDepthY >= 0 && iColorInDepthY < iDepthHeight)
                                        {
                                            var iIdx = (pColourIndex.X + (pColourIndex.Y * iColourWidth)) * 4;
                                            tInterfacePixels[iInterfacePtr + 0] = tColourPixels[iIdx + 0];
                                            tInterfacePixels[iInterfacePtr + 1] = tColourPixels[iIdx + 1];
                                            tInterfacePixels[iInterfacePtr + 2] = tColourPixels[iIdx + 2];
                                        }
                                        else
                                        {
                                            // Write white.
                                            tInterfacePixels[iInterfacePtr + 0] = 0xFF;
                                            tInterfacePixels[iInterfacePtr + 1] = 0xFF;
                                            tInterfacePixels[iInterfacePtr + 2] = 0xFF;
                                        }
                                    }
                                    break;
                                case RenderStrategy.GrayscaleDepth:
                                    Utilities.ColourUtils.Greyscale(pDepthPixel.Depth, 0, 3000, ref tInterfacePixels[iInterfacePtr + 0], ref tInterfacePixels[iInterfacePtr + 1], ref tInterfacePixels[iInterfacePtr + 2]);
                                    break;
                                case RenderStrategy.RGBDepth:
                                    Utilities.ColourUtils.FauxColourRGB(pDepthPixel.Depth, 0, 3000, ref tInterfacePixels[iInterfacePtr + 0], ref tInterfacePixels[iInterfacePtr + 1], ref tInterfacePixels[iInterfacePtr + 2]);
                                    break;
                                case RenderStrategy.RGBDepthRelative:
                                    Utilities.ColourUtils.FauxColourRGB(Utilities.RatcliffPlane.Distance(tPointCloud[iPointCloudCount], tRenderPlane), 0, 3.0, ref tInterfacePixels[iInterfacePtr + 0], ref tInterfacePixels[iInterfacePtr + 1], ref tInterfacePixels[iInterfacePtr + 2]);
                                    break;
                            }
                        }

                        // Store the point cloud count.
                        pPointCloud.Next.First = iPointCloudCount;
                    }

                    // Release access to the image and point cloud buffer.
                    pInterfacePixels.UnlockNextAndFlip();   // WARNING - this may wait for the UI thread to finsih reading.
                    pPointCloud.UnlockNextAndFlip();        // WARNING - this may wait for the point cloud thread to finsih reading.
                }
            }
            #endregion

            // Release access to the image buffer.
            //pInterfacePixels.UnlockNextAndFlip();   // WARNING - this may wait for the UI thread to finsih reading.
            //pPointCloud.UnlockNextAndFlip();        // WARNING - this may wait for the point cloud thread to finsih reading.

            // Raise the event which says we are done.
            if (OnFrameReady != null)
                OnFrameReady(this);

            // Signal that the frame has been processed.
            _FPSCounter.NewFrame();

            // Signal that we have begun streaming.
            if (!bStreamedFirstFrame)
            {
                bStreamedFirstFrame = true;
                if (OnKinectStreamingStarted != null)
                    OnKinectStreamingStarted(this);
            }
        }


        /// <summary>
        /// Copy the contents of the current image buffer into a writable bitmap.
        /// </summary>
        /// <remarks>The bitmap size is used to known how much to copy.  It assumes 4bpp.</remarks>
        /// <param name="pBitmap">The bitmap to write to.</param>
        /// <param name="iLockWait">How long we want to wait to acquire access to the current buffer before skipping.</param>
        /// <returns>True if the lock was acquired and the data copied.  False if not.</returns>
        public bool ImageToWriteableBitmap(WriteableBitmap pBitmap, int iLockWait = 100)
        {
            // Bail if we are not active.
            if (!IsActive)
                return false;

            // Attempt to get a lock.
            if (pInterfacePixels.TryLockCurrent(iLockWait))
            {
                // Write the pixels into the bitmap.
                pBitmap.WritePixels(
                    new Int32Rect(0, 0, pBitmap.PixelWidth, pBitmap.PixelHeight),
                    this.pInterfacePixels.Current,
                    pBitmap.PixelWidth * 4,
                    0);

                // Unlock the buffer.
                pInterfacePixels.UnlockCurrent();

                // Return success!
                return true;
            }

            // Return failure, we were not able to obtain a lock.
            return false;
        }

        /// <summary>
        /// Copy the contents of the current image buffer into a pixel array.
        /// </summary>
        /// <param name="tDestination">The array of bytes to write into.</param>
        /// <param name="iLockWait">How long we want to wait to acquire access to the current buffer before skipping.</param>
        /// <returns>True if the lock was acquired and the data copied.  False if not.</returns>
        public bool ImageToArray(byte[] tDestination, int iLockWait = 100)
        {
            // Bail if we are not active.
            if (!IsActive)
                return false;

            // Attempt to get a lock.
            if (pInterfacePixels.TryLockCurrent(iLockWait))
            {
                // Copy the pixels.
                Array.Copy(pInterfacePixels.Current, tDestination, pInterfacePixels.Current.Length);

                // Unlock the buffer.
                pInterfacePixels.UnlockCurrent();

                // Return success!
                return true;
            }

            // Return failure, we were not able to obtain a lock.
            return false;
        }

        /// <summary>
        /// Get the 3D point from the UI image given coordinates in the depth image.
        /// </summary>
        /// <remarks>This will throw an exception if x and y are out of bounds.</remarks>
        /// <param name="x">The x-coordinate of the image.</param>
        /// <param name="y">The y-coordinate of the image.</param>
        /// <returns>The 3D point at that location.</returns>
        public SlimMath.Vector3 ImageTo3DDepth(int x, int y)
        {
            // Bail if we are not active.
            if (!IsActive)
                throw new Exception("Cannot access data before Kinect is started.");

            // Create spmewhere to put the data.
            var vPoint = new SlimMath.Vector3();

            // Get the 3D point from the index to the image.
            var idx = x + (y * iDepthWidth);

            // Acquire the index buffer lock.
            mIndexLock.WaitOne();

            // If we have an error.
            if (idx < 0 || idx >= (iDepthWidth*iDepthHeight))
            {
                mIndexLock.ReleaseMutex();
                throw new ArgumentOutOfRangeException("Cannot access 3D depth at image point ("+x+","+y+").  Out of bounds.");
            }

            // Copy the point.
            vPoint.X = tDepthToSkeleton[idx].X;
            vPoint.Y = tDepthToSkeleton[idx].Y;
            vPoint.Z = tDepthToSkeleton[idx].Z;

            // Release the lock.
            mIndexLock.ReleaseMutex();

            // Return the 3D depth.
            return vPoint;
        }

        /// <summary>
        /// Get a list of 3D points which correspond to coordinates in the depth image.
        /// </summary>
        /// <remarks>This will throw an exception if x and y are out of bounds.</remarks>
        /// <param name="lPoints">The list of input points (x,y).</param>
        /// <returns>The 3D point at that location.</returns>
        public List<SlimMath.Vector3> ImagePointsTo3DDepth(IEnumerable<System.Windows.Point> lPoints)
        {
            // Bail if we are not active.
            if (!IsActive)
                throw new Exception("Cannot access data before Kinect is started.");

            // Create somewhere to put the 3d output data.
            var lOutput = new List<SlimMath.Vector3>();

            // Acquire the index buffer lock.
            mIndexLock.WaitOne();

            // Compute the buffer size.
            var iSize = (iDepthWidth * iDepthHeight);

            // For each input point, create a corresponding output point.
            foreach (var pPoint in lPoints)
            {
                // Get the 3D point from the index to the image.
                var idx = (((int)pPoint.X) + (((int)pPoint.Y) * iDepthWidth));

                // If we have an error.
                if (idx < 0 || idx >= iSize)
                {
                    mIndexLock.ReleaseMutex();
                    throw new ArgumentOutOfRangeException("Cannot access 3D depth at image point (" + pPoint.X + "," + pPoint.Y + ").  Out of bounds.");
                }

                // Copy the point.
                lOutput.Add(new SlimMath.Vector3( tDepthToSkeleton[idx].X, tDepthToSkeleton[idx].Y, tDepthToSkeleton[idx].Z ));
            }

            // Release the lock.
            mIndexLock.ReleaseMutex();

            // Return the 3D depth.
            return lOutput;
        }

        /// <summary>
        /// Get the colour from the UI image at given coordinates in the depth image.
        /// </summary>
        /// <remarks>This will throw an exception if x and y are out of bounds.</remarks>
        /// <param name="x">The x-coordinate of the image.</param>
        /// <param name="y">The y-coordinate of the image.</param>
        /// <returns>The colour at that location in the depth image.</returns>
        public System.Windows.Media.Color ImageToColour(int x, int y)
        {
            // Bail if we are not active.
            if (!IsActive)
                throw new Exception("Cannot access data before Kinect is started.");

            // Create spmewhere to put the data.
            var pColour = new System.Windows.Media.Color();

            // Get the 3D point from the index to the image.
            var idx = x + (y * iDepthWidth);

            // Acquire the index buffer lock.
            mIndexLock.WaitOne();

            // If we have an error.
            if (idx < 0 || idx >= (iDepthWidth*iDepthHeight))
            {
                mIndexLock.ReleaseMutex();
                throw new ArgumentOutOfRangeException("Cannot access 3D depth at image point (" + x + "," + y + ").  Out of bounds.");
            }

            // Get the colour point.
            ColorImagePoint pColourIndex = tDepthToColourIB[idx];
            int colorInDepthX = pColourIndex.X / this.iColourToDepthDivisor;
            int colorInDepthY = pColourIndex.Y / this.iColourToDepthDivisor;
            if (colorInDepthX > 0 && colorInDepthX < iDepthWidth && colorInDepthY >= 0 && colorInDepthY < iDepthHeight)
            {
                var iIdx = (pColourIndex.X + (pColourIndex.Y * iColourWidth)) * 4;
                pColour.B = tColourPixels[iIdx + 0];
                pColour.G = tColourPixels[iIdx + 1];
                pColour.R = tColourPixels[iIdx + 2];
            }
            else
            {
                pColour = System.Windows.Media.Colors.White;
            }

            // Release the lock.
            mIndexLock.ReleaseMutex();

            // Return the 3D depth.
            return pColour;
        }

        /// <summary>
        /// Compute a best-fit plane for all the 3D points which lie in a certain area of the image.
        /// </summary>
        /// <remarks>This is probably not the speediest function in the world because it needs to do some rasterising, polygon testing, and best-fit-plane computation.  Smaller polygons are obvs better!</remarks>
        /// <param name="tImagePolygon">The ordered corners of the polygon in depth image space.</param>
        /// <returns>The 3D plane which best fits all the 3D points tested.</returns>
        public SlimMath.Plane ImagePolygonToBestFitPlane(Point[] tImagePolygon)
        {
            // Get the image x,y coordinates which are involved in the polygon.
            #region Polygon Output
            List<Point> lRasterised = new List<Point>();

            // Compute the bounding box for the polygon.
            int iCount = tImagePolygon.Length;
            if (iCount < 3)
                throw new Exception("Not enough points to construct a polygon.");

            // Compute the max and min of the polygon so we can limit the processing.
            int xMax = (int)tImagePolygon[0].X;
            int xMin = (int)tImagePolygon[0].X;
            int yMax = (int)tImagePolygon[0].Y;
            int yMin = (int)tImagePolygon[0].Y;
            for (int i = 1; i < iCount; ++i)
            {
                xMax = (int)Math.Max(tImagePolygon[i].X, xMax);
                yMax = (int)Math.Max(tImagePolygon[i].Y, yMax);

                xMin = (int)Math.Min(tImagePolygon[i].X, xMin);
                yMin = (int)Math.Min(tImagePolygon[i].Y, yMin);
            }

            // Test if each pixel in the bounding box lies in the polygon.
            for (int y = yMin; y < yMax; ++y)
            {
                for (int x = xMin; x < xMax; ++x)
                {
                    // Set the bit if the point is in the polygon.
                    if (Utilities.Polygon.IsPointInPolygon((double)x, (double)y, tImagePolygon))
                    {
                        lRasterised.Add(new Point(x, y));
                    }
                }
            }
            #endregion

            // Get the 3D depth for each point we rasterised.
            var tPoints = this.ImagePointsTo3DDepth(lRasterised).ToArray();

            // Compute the best fit plane of these points.
            return Utilities.RatcliffPlane.ComputeBestFit(tPoints);
        }

        /// <summary>
        /// Compute a best-fit plane for all the 3D points which lie in a certain area of the image.
        /// </summary>
        /// <remarks>This is probably not the speediest function in the world because it needs to do some rasterising, polygon testing, and best-fit-plane computation.  Smaller polygons are obvs better!</remarks>
        /// <param name="tImagePoints">The list of image points to test.  For instance, the four corners of a web page.</param>
        /// <returns>The 3D plane which best fits all the 3D points tested.</returns>
        public SlimMath.Plane ImageCoordinatesToBestFitPlane(Point[] tImagePoints)
        {
            // Get the 3D depth for each point we rasterised.
            var tPoints = this.ImagePointsTo3DDepth(tImagePoints).ToArray();

            // Compute the best fit plane of these points.
            return Utilities.RatcliffPlane.ComputeBestFit(tPoints);
        }

        /// <summary>
        /// Create a copy of the point cloud in the current buffer.
        /// </summary>
        /// <param name="iLockWait">The amount of time to wait before giving up.  The buffer might be locked.</param>
        /// <returns>A copy of the data in the current buffer.</returns>
        public SlimMath.Vector3[] CopyPointCloud(int iLockWait = 100)
        {
            // Bail if we are not active.
            if (!IsActive)
                return null;

            // Attempt to get a lock
            if (pPointCloud.TryLockCurrent(iLockWait))
            {
                // Take the first n values (i.e. the valid ones) from this point cloud.
                var tCopy = new SlimMath.Vector3[pPointCloud.Current.First];
                Array.Copy(pPointCloud.Current.Second, tCopy, tCopy.Length);

                // Unlock the buffer.
                pPointCloud.UnlockCurrent();

                // Return success!
                return tCopy;
            }

            // Nothing to do so return.
            return null;
        }

        /// <summary>
        /// Run a series of spatial queries against this point cloud. NB. You need to call BeginFrame() and EndFrame() yourself.
        /// </summary>
        /// <param name="lQueries">The list of queries to run.</param>
        /// <param name="tDebugFrame">An array (the same dimensions as the depth frame at 4Bpp BRGA) which we want to write debug data into.</param>
        /// <param name="iLockWait">The amount of time to wait before giving up on being able to access the buffer.  Milliseconds.</param>
        public void QueryPointCloud(IEnumerable<ISpatialQuery> lQueries, byte[] tDebugFrame = null, int iLockWait = 100)
        {
            // Bail if we are not active.
            if (!IsActive)
                return;

            // Attempt to get a lock
            if (pPointCloud.TryLockCurrent(iLockWait))
            {
                // Bring some vars into scope.
                var tPCIndex = pPointCloud.Current.Third;
                var tPCData  = pPointCloud.Current.Second;
                var iPCLimit = pPointCloud.Current.First;

                // Iterate for no debug data.
                if (tDebugFrame == null)
                {
                    // For each query.
                    foreach (var pQuery in lQueries)
                    {
                        for (int i = 0; i < iPCLimit; ++i)
                            pQuery.InsertIfContained(tPCData[i]);
                    }
                }
                else
                {
                    // For each query.
                    foreach (var pQuery in lQueries)
                    {
                        for (int i = 0; i < iPCLimit; ++i)
                        {
                            if (pQuery.InsertIfContained(tPCData[i]))
                            {
                                tDebugFrame[(tPCIndex[i]) + 0] = 0x00; // B
                                tDebugFrame[(tPCIndex[i]) + 1] = 0x00; // G
                                tDebugFrame[(tPCIndex[i]) + 2] = 0xFF; // R
                                tDebugFrame[(tPCIndex[i]) + 3] = 0xCC; // A
                            }
                        }
                    }
                }

                // Unlock the buffer.
                pPointCloud.UnlockCurrent();
            }

            // Nothing to do so return.
            return;
        }

        // TODO
        // Add ImageColourToTransformedWritableBitmap (wb, homography, corners) -- for each pixel in bitmap, inverse homography, take colour, store, repeat.

    }
}
