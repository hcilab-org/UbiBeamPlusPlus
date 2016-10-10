# UbiBeam++


##Installation Guide
Our System consists of two applications: One the one hand the server application controlling the course of the game and on the other hand the client application which is responsible for displaying the hand cards and the three-dimensional representation of the units placed on the game field.

Server Application
------

### System Requirements

For running the server application the following hardware is needed:

* projector
* Microsoft Kinect for Windows v1

Furthermore, the following software must be installed on the computer running the server application:

* .NET Framework 4.5
* Microsoft Visual Studio 2013
* UbiDisplays for the calibrating the Kinect

### Calibration of the Kinect
In order to detect user interactions of a flat surface e.g. the top of a desk, the Kinect must be calibrated.

1. First run the application “UbiDisplays”.
2. In the first tab of the window select the screen which is shown on the projector and select the Kinect which is connected to the computer.
3. Select the points on the projected surface by clicking on them in the video preview.
4. Select the surface on which user interaction should be detected:
    * First click on “Draw Surface” and then draw a line on the video preview. 
    * After that you can adjust the covered rectangle by dragging its corners
    * In the end the whole projection should be covered.
5. Select the orientation to match the one of the operating system.
6. Import the .html file including the heights of the detected volume by drag and drop on the video preview.
7. Check the quality of detection: 
    * If there is too much noise on the surface increase the lower threshold.
    * If there is too much noise on the surface increase the lower threshold.
    * If there is an offset between the real fingertip and its detection decrease the lower threshold.
8. Save the configuration file named “Untitled.ubi” into the root directory of the server application.

### Start of the Application
1. Copy all files into a directory where you have write permissions.
2. Run the executable “UbiBeamPlusPlus.exe”
3. Select the directory containing the cards to be used in the game. These cards can be found in the subdirectory “cards”.

Client Application
-----

### System Requirements
For each player one Meta1 and one computer with HDMI output is needed.
Furthermore, the Meta1 SDK must be downloaded and installed on these computers.

### Preparation
The Meta1 must be connected to its control box and this control box must be connected to the client computer using HDMI and USB. After that the power cable can be plugged into the control box to start the Meta1.

### Calibration of the Meta1
Start the calibration application provided by the Meta SDK and follow it’s instructions.

### Start of the Application
1. Run the executable on the client computer.
2. In the Meta Configuration window select 1280x720 for the resolution and uncheck “windowed”. 
3. Click “Play!” to start the game.

## Why citing UbiBeam++

If you are using UbiBeam++ in your research-related documents, it is recommended that you cite UbiBeam++. This way, other researchers can better understand your proposed-method. Your method is more reproducible and thus gaining better credibility.

## How to cite UbiBeam++

Below is the BibTex entry for citing UbiBeam++

<pre>
@inproceedings{Knierim:2016,
  author = {Pascal Knierim, Markus Funk, Thomas Kosch, Anton Fedosov, Tamara Müller, Benjamin Schopf, Marc Weise and Albrecht Schmidt},
  title = {UbiBeam++: Augmenting Interactive Projection with Head-Mounted Displays},
  booktitle = {Proceedings of the 9th Nordic Conference on Human-Computer Interaction}
  series = {NordiCHI '16},
  year = {2016},
  location = {Gothenburg, Sweden},
  url = {http://doi.acm.org/10.1145/2971485.2996747},
  doi = {10.1145/2971485.2996747},
  acmid = {2996747},
  publisher = {ACM},
  address = {New York, NY, USA},
}
</pre>