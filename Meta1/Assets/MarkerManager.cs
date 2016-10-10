using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Meta;
using System.IO;

public class MarkerManager : MonoBehaviour {

    //playerID can be 0 or 1
    private int player;

    //show virtual marker?
    private bool markerTargetIndicator = false;

    //GameObjects for the 3D models and the text on the field
    private Dictionary<int, GameObject> markersDictionary;

    //saves all paths to the cards/models
    private Dictionary<int, string> cardsPathDictionary;
    private Dictionary<int, string> modelsPathDictionary;

    //stores all deviating sizes for the 3D models
    private Dictionary<int, float[]> modelsSizeDictionary;

    //set playerID at Unity
    public bool player1;

    //max index of marker
    int max_markerID = 586;

    //max number of cards
    int max_cards = 5;

    //show values
    private bool ownVisible = false;
    private bool otherVisible = false;

    //initialization
    void Start() {
        //set player 0 or 1
        if (player1) {
            player = 1;
        } else {
            player = 0;
        }

        //initialize loom for calls in main thread
        Loom.Initialize();

        //initialize dictionary for all parent GameObjects with markers
        //key = marker ID
        markersDictionary = new Dictionary<int, GameObject>();

        //loads all paths to cards and models
        loadCards();
        loadModels();

        //saves deviating model sizes
        //normal scaling 0.05F, 0.05F, 0.05F
        modelsSizeDictionary = new Dictionary<int, float[]>();
        modelsSizeDictionary.Add(4, new float[] { 0.02F, 0.04F, 0.04F });
        modelsSizeDictionary.Add(10, new float[] { 0.25F, 0.25F, 0.25F });
        modelsSizeDictionary.Add(11, new float[] { 0.12F, 0.12F, 0.12F });
        modelsSizeDictionary.Add(12, new float[] { 0.014F, 0.014F, 0.014F });
        //golem
        modelsSizeDictionary.Add(3, new float[] { 0.095F, 0.095F, 0.095F });
        //robot
        //modelsSizeDictionary.Add(, new float[] { 0.07F, 0.07F, 0.07F });
    }

    /*
     * Returns maximum number of cards.
     */
    public int getMaxCards() {
        return this.max_cards;
    }

    /*
     * Returns max markerID as int.
     */
    public int getMaxMarkerID() {
        return this.max_markerID;
    }

    /*
     * Starts a new game on the meta1.
     */
    public void startNewGame() {

        Loom.QueueOnMainThread(() => {
            //set all values not visible
            ownVisible = false;
            otherVisible = false;

            //delete all GameObjects
            foreach (var pair in markersDictionary) {
                Destroy(pair.Value);
                Debug.Log("Delete: " + pair.Value.name);
            }

            //delete all entries in the markersDictionary
            markersDictionary.Clear();

            Debug.Log("New game started!");
        });
    }

    /*
     * Checks if the cardID is valid.
     */
    public bool existsCardIDForModel(int id) {
        return modelsPathDictionary.ContainsKey(id);
    }

    /*
     * Create cards and show them on meta1.
     */
    public void showCards(int[] cardArray) {
        Debug.Log("Create card!");
        //work on main thread
        Loom.QueueOnMainThread(() => {
            //check if the key is already in the dictionary
            if (markersDictionary.ContainsKey(player)) {
                //delete old GameObject
                Destroy(markersDictionary[player]);
                markersDictionary[player] = (GameObject)Instantiate(Resources.Load("Cards"));
            } else {
                //new GameObject for cards
                markersDictionary.Add(player, (GameObject)Instantiate(Resources.Load("Cards")));
            }
            //rename
            markersDictionary[player].transform.name = "Cards";

            int cardIndex;

            if (player1) {
                cardIndex = 1;
            } else {
                cardIndex = this.max_cards;
            }

            //create all visible cards and delete unused GameObjects where no cards
            for (int i = 0; i < this.max_cards; i++) {

                //get cardID from cardArray to show card
                if (i < cardArray.Length) {
                    //check if there is a card at this place
                    if (cardArray[i] == -1) {
                        //no card at this place
                        //delete GameObject at this place
                        deleteCard(cardIndex);

                    } else {
                        //show card at this place
                        //map cardID to picture
                        string cardPath = this.mapIntToPath(cardArray[i], false);

                        //check if card path exists
                        if (cardPath == null) {
                            //delete card because there is no path to the card image
                            deleteCard(cardIndex);
                            Debug.Log("No valid cardID!");

                        } else {
                            //create card
                            //get card GameObject
                            GameObject card = markersDictionary[player].transform.FindChild("Hand").FindChild("Card" + cardIndex).gameObject;

                            //get mesh renderer
                            Renderer renderer = card.GetComponent<MeshRenderer>().renderer;
                            //get texture for card
                            Texture texture = Resources.Load(cardPath) as Texture;
                            //set new texture to mesh renderer
                            renderer.material.mainTexture = texture;
                            //load shader which supports transparency
                            //add to 'Always included shaders' in the project settings
                            renderer.material.shader = Shader.Find("Transparent/Cutout/Soft Edge Unlit");
                        }
                    }

                    //no card at this place, because array doesn't have so much entries
                } else {
                    //delete GameObject at this place
                    deleteCard(cardIndex);
                }

                //increment/decrement cardIndex
                if (player1) {
                    cardIndex++;
                } else {
                    cardIndex--;
                }
                Debug.Log("cardIndex = " + cardIndex);
            }
            //hide GameObject in scene to fix short popup, when marker isn't visible
            markersDictionary[player].transform.FindChild("Hand").gameObject.SetActive(false);

            //enable tracking for 'Cards' GameObject
            enableTracking(markersDictionary[player], player);
        });
    }

    /*
     * Deletes the card at the given index.
     * cardIndex: 1-5
     */
    private void deleteCard(int cardIndex) {
        GameObject card = markersDictionary[player].transform.FindChild("Hand").FindChild("Card" + cardIndex).gameObject;
        Destroy(card);
    }

    /*
     * Add 3D model and text for the game field.
     */
    public void addModel(int playerID, int markerID, int cardID, int[] values) {
        bool newModel = true;

        Debug.Log("Create model!");

        //work on main thread
        Loom.QueueOnMainThread(() => {

            //check if the key is already in the dictionary
            if (markersDictionary.ContainsKey(markerID)) {
                //get previous cardID and playerID from string 'PlayerXCardY'
                string oldName = markersDictionary[markerID].transform.name;
                int oldPlayerID = int.Parse(oldName.Substring(6, 1));
                int oldCardID = int.Parse(oldName.Substring(11));

                //check if values changed or hole model
                if (oldCardID == cardID) {
                    bool playerChanged = false;

                    //check if player changed
                    if (oldPlayerID != playerID) {
                        playerChanged = true;
                        //player changed, rename GameObject
                        markersDictionary[markerID].name = "Player" + playerID + "Card" + cardID;
                    }

                    //update values
                    newModel = false;
                    //get old values GameObject
                    GameObject valuesObject = markersDictionary[markerID].transform.FindChild("Values").gameObject;
                    //need to rotate values?
                    if (playerChanged) {
                        //rotate GameObject 180°
                        valuesObject.transform.rotation *= Quaternion.AngleAxis(180, transform.up);
                    }
                    //values visible for player?
                    if (playerID == player) {
                        valuesObject.SetActive(this.ownVisible);
                    } else {
                        valuesObject.SetActive(this.otherVisible);
                    }
                    updateValues(valuesObject, values);

                } else {
                    //change hole model
                    //delete old GameObject and create new one
                    Destroy(markersDictionary[markerID]);
                    markersDictionary[markerID] = new GameObject("Player" + playerID + "Card" + cardID);
                }

            } else {
                //create new GameObject
                markersDictionary.Add(markerID, new GameObject("Player" + playerID + "Card" + cardID));
            }

            if (newModel) {
                //get model path by card id
                string path = mapIntToPath(cardID, true);
                //create model
                Debug.Log("Instantiate: " + path);
                GameObject model = (GameObject)Instantiate(Resources.Load(path));
                model.transform.name = "3D";
                //add model as child
                model.transform.SetParent(markersDictionary[markerID].transform);
                //scale model
                if (modelsSizeDictionary.ContainsKey(cardID)) {
                    float[] tempSize = modelsSizeDictionary[cardID];
                    model.transform.localScale = new Vector3(tempSize[0], tempSize[1], tempSize[2]);
                } else {
                    model.transform.localScale = new Vector3(0.05F, 0.05F, 0.05F);
                }

                //show values: health, countdown, damage
                //create values GameObject
                GameObject valuesObject = (GameObject)Instantiate(Resources.Load("Values"));
                //rename values GameObject
                valuesObject.transform.name = "Values";
                //add values GameObject as child
                valuesObject.transform.SetParent(markersDictionary[markerID].transform);
                //values visible for player?
                if (playerID == player) {
                    valuesObject.SetActive(this.ownVisible);
                    //position relative to marker
                    // -z and -x
                    Vector3 vec = new Vector3(-0.01F, 0, -0.015F);
                    valuesObject.transform.position += vec;
                    model.transform.position += vec;
                } else {
                    valuesObject.SetActive(this.otherVisible);
                    //rotate GameObject 180°
                    valuesObject.transform.rotation *= Quaternion.AngleAxis(180, transform.up);
                    //position relative to marker
                    // -z and -x
                    Vector3 vec = new Vector3(0.005F, 0, 0.03F);
                    valuesObject.transform.position += vec;
                    model.transform.position += vec;
                }
                //set values to GameObject
                updateValues(valuesObject, values);

                //hide GameObject in scene to fix short popup, when marker isn't visible
                markersDictionary[markerID].SetActive(false);

                //enable tracking for new marker
                enableTracking(markersDictionary[markerID], markerID);
            }
        });
    }

    /*
     * Shows the values for the players or hide them.
     * 
     * int valuesFromPlayer: number of the player the values should be shown/hidden
     * int showPlayers: show only the player which the values belong (0) or show all players (1)
     * bool visible: show values (true) or hide values (false)
     */
    public void showValues(int valuesFromPlayer, int showPlayers, bool visible) {

        //work on main thread
        Loom.QueueOnMainThread(() => {

            //check if this are my own values
            if (valuesFromPlayer == player) {
                //own values
                //set global value
                this.ownVisible = visible;
                //show/hide values
                showHideValues(true);

            } else {
                //values from other player
                //should be shown/hidden for me?
                if (showPlayers == 1) {
                    //set global value
                    this.otherVisible = visible;
                    //show/hide values
                    showHideValues(false);
                }
            }
        });
    }

    /*
     * Show or hide values over the 3D model.
     */
    private void showHideValues(bool own) {

        //show/hide values from player
        foreach (var pair in markersDictionary) {
            //check if marker represents models
            if (pair.Key > 1) {
                GameObject go = pair.Value;
                //get player of GameObject
                int currentPlayer = int.Parse(go.name.Substring(6, 1));
                //check if itn's my own GameObject
                if (own) {
                    if (currentPlayer == player) {
                        //find GameObject an show/hide values
                        GameObject val = go.transform.FindChild("Values").gameObject;
                        val.SetActive(this.ownVisible);
                    }
                } else {
                    if (currentPlayer != player) {
                        //find GameObject an show/hide values
                        GameObject val = go.transform.FindChild("Values").gameObject;
                        val.SetActive(this.otherVisible);
                    }
                }
            }
        }
    }

    /*
     * Updates the values over the 3D models.
     */
    private void updateValues(GameObject valuesObject, int[] values) {
        //get panel
        GameObject panel = valuesObject.transform.FindChild("Panel").gameObject;
        //add each value
        //health
        GameObject health = panel.transform.FindChild("Health_Value").gameObject;
        UnityEngine.UI.Text health_value = health.GetComponent<UnityEngine.UI.Text>();
        health_value.text = values[0].ToString();
        //countdown
        GameObject countdown = panel.transform.FindChild("Countdown_Value").gameObject;
        UnityEngine.UI.Text countdown_value = countdown.GetComponent<UnityEngine.UI.Text>();
        countdown_value.text = values[1].ToString();
        //damage
        GameObject damage = panel.transform.FindChild("Damage_Value").gameObject;
        UnityEngine.UI.Text damage_value = damage.GetComponent<UnityEngine.UI.Text>();
        damage_value.text = values[2].ToString();
    }

    /*
     * Maps cardID to card/model path.
     * 
     * return: path to the card/model or null.
     */
    private string mapIntToPath(int id, bool threeD) {
        string path;

        //check if a 3D models is searched or a card
        if (threeD) {
            //3D model
            //generate path to model
            path = "Models/";
            //get file name by id
            if (modelsPathDictionary.ContainsKey(id)) {
                path += modelsPathDictionary[id];
            } else {
                // TODO default model?
                Debug.Log("No model exists for this cardID!");
                return null;
            }
        } else {
            //card
            //generate path to card
            path = "Cards/";
            //get file name by id
            if (cardsPathDictionary.ContainsKey(id)) {
                path += cardsPathDictionary[id];
            } else {
                // TODO default card?
                Debug.Log("No card exists for this cardID!");
                return null;
            }
        }

        return path;
    }

    /*
     * Loads all cards in the Resources/Cards folder.
     */
    private void loadCards() {

        string[] temp = getAllFilesFromPath("Cards", true);

        //create dictionary for paths to the cards
        cardsPathDictionary = new Dictionary<int, string>();

        //fill global dictionary
        for (int i = 0; i < temp.Length; i++) {
            //delete file type
            temp[i] = temp[i].Substring(0, temp[i].Length - 4);
            //get id of card
            //split array at '_'
            string[] splitted = temp[i].Split(new char[] { '_' });
            //last value contains id
            int index = int.Parse(splitted[splitted.Length - 1]);
            //save path to the dictionary
            cardsPathDictionary.Add(index, temp[i]);

            Debug.Log("cardID: " + index + "  |  " + temp[i]);
        }
    }

    private void loadModels() {
        string[] temp = getAllFilesFromPath("Models", false);

        //create dictionary for paths to the models
        modelsPathDictionary = new Dictionary<int, string>();

        //fill global dictionary
        for (int i = 0; i < temp.Length; i++) {

            //get path to models
            string[] splittedPath = temp[i].Split(new char[] { '\\' });
            int count = 0;
            string tempPath = "";
            foreach (string s in splittedPath) {
                if (count == 2) {
                    tempPath += s + "/";
                } else {
                    if (s.Equals("Resources") || s.Equals("Models")) {
                        count++;
                    }
                }
            }
            //delete last '/' and save path without file ending
            if (temp[i].EndsWith(".prefab")) {
                temp[i] = tempPath.Substring(0, tempPath.Length - 8);
            } else {
                temp[i] = tempPath.Substring(0, tempPath.Length - 5);
            }

            Debug.Log("-> " + temp[i]);

            //get id of card
            //split string by '_'
            string[] splitted = temp[i].Split(new char[] { '_' });
            //last index contains id
            int index = int.Parse(splitted[splitted.Length - 1]);
            //save path to the dictionary
            modelsPathDictionary.Add(index, temp[i]);

            Debug.Log("cardID: " + index + "  |  " + temp[i]);
        }
    }

    private string[] getAllFilesFromPath(string path, bool isCard) {

        string projectPath = "";

        //check if player mode is running
        if (Application.platform == RuntimePlatform.WindowsPlayer) {
            //load file names from project folder, not from standalone version
            int lastSlash = Application.dataPath.LastIndexOf('/');
            projectPath = Application.dataPath.Substring(0, lastSlash) + "\\Assets";

        } else {
            //check if editor mode is running
            if (Application.platform == RuntimePlatform.WindowsEditor) {
                projectPath = Application.dataPath;
            } else {
                Debug.Log("No supported runtime platform!");
                return null;
            }
        }

        //get all files in 'Resources' folder of Unity project
        DirectoryInfo levelDirectoryPath = new DirectoryInfo(projectPath + "\\Resources\\" + path);
        string[] temp;
        int count = 0;

        if (isCard) {
            FileInfo[] fileInfo = levelDirectoryPath.GetFiles("*.*", SearchOption.TopDirectoryOnly);
            //can contain more than two _
            Regex rgx = new Regex("\\w*_ID_\\d+.png$");

            temp = new string[fileInfo.Length];

            foreach (FileInfo file in fileInfo) {
                if (file.Extension == ".png") {
                    if (rgx.IsMatch(file.Name)) {
                        temp[count] = file.Name;
                        count++;
                    } else {
                        //card has no valid file name
                        Debug.Log("No match for card: " + file.Name);
                    }
                }

            }
        } else {
            //is 3D model
            FileInfo[] fileInfo = levelDirectoryPath.GetFiles("*.*", SearchOption.AllDirectories);
            //can contain more than two _
            Regex rgx = new Regex("\\w*_ID_\\d+.(fbx|FBX)$");
            Regex rgx2 = new Regex("\\w*_ID_\\d+.prefab$");

            temp = new string[fileInfo.Length];

            foreach (FileInfo file in fileInfo) {
                if (file.Extension == ".fbx" | file.Extension == ".FBX") {
                    if (rgx.IsMatch(file.Name)) {
                        temp[count] = file.FullName;
                        count++;
                    } else {
                        //model has no valid file name
                        Debug.Log("No match for model: " + file.Name);
                    }
                } else {
                    if (file.Extension == ".prefab") {
                        if (rgx2.IsMatch(file.Name)) {
                            temp[count] = file.FullName;
                            count++;
                        } else {
                            //model has no valid file name
                            Debug.Log("No match for model: " + file.Name);
                        }
                    }
                }
            }
        }

        Debug.Log("#: " + count);

        //check if temp has no empty entries
        if (count == temp.Length) {
            return temp;
        }
        //add all vaild paths to new array
        string[] result = new string[count];
        for (int i = 0; i < count; i++) {
            result[i] = temp[i];
        }
        return result;
    }

    /*
     * Enables tracking for an GameObject.
     */
    private void enableTracking(GameObject obj, int markerID) {
        //add MetaBody
        MetaBody metaBodyComponent = obj.AddComponent<MetaBody>();
        //enable tracking
        metaBodyComponent.markerTarget = true;
        //set the marker id we are looking for
        metaBodyComponent.markerTargetID = markerID;
        //turn off the marker target indicator
        MarkerDetector.Instance.gameObject.GetComponent<MarkerTargetIndicator>().enabled = markerTargetIndicator;
    }

    /*
     * Check the marker is in sight of the camera
     */
    bool LookForMarker(int id) {
        //Check that markers are in use
        if (MarkerDetector.Instance != null) {
            //Check if the marker we want exists
            if (MarkerDetector.Instance.updatedMarkerTransforms.Contains(id)) {
                return true;
            }
        }
        return false;
    }

    /*
     * Check for each marker if it is visible or not.
     */
    void Update() {

        //exists a marker?
        if (markersDictionary.Count > 0) {

            //check for cards marker
            if (markersDictionary.ContainsKey(player)) {
                //can we see the marker?
                if (LookForMarker(player)) {
                    markersDictionary[player].transform.FindChild("Hand").gameObject.SetActive(true);
                } else {
                    markersDictionary[player].transform.FindChild("Hand").gameObject.SetActive(false);
                }
            }

            //go through all markers
            foreach (var pair in markersDictionary) {
                //get id
                int id = pair.Key;
                //represents the id a model?
                if (id > 1) {
                    //can we see the marker?
                    if (LookForMarker(id)) {
                        markersDictionary[id].SetActive(true);
                    } else {
                        markersDictionary[id].SetActive(false);
                    }

                }
            }
        }
    }
}
