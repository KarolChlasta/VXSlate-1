﻿using Assets.Script.TestGame;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PuzzleMaker : MonoBehaviour
{
    public bool _isInit = false;

    private const float _coefficientCoverterPixelToM = 3779.5275590551f;

    public GameObject _largeScreenObject;
    // Large screen size in pixels
    float _largeScreenWidth;
    float _largeScreenHeight;

    public Material _screenMaterial;

    public Texture2D _puzzle1O;
    public Texture2D _puzzle1L;
    public Texture2D _puzzle2O;
    public Texture2D _puzzle2L;
    public Texture2D _startButtonTexture;

    Texture2D _mainPuzzleTexture;
    Texture2D _drawPuzzleTexture;
    Texture2D _originalPuzzleTexture;

    public Texture2D _gridPieceTexture;

    float _percentageForMargins = 0.05f;
    float _marginSize = 0;

    int _x = 4; // number of collumn
    int _y = 4; // number of rows

    Vector2 _largeScreenCenter; //in pixel
    Vector2 _sampleScreenCenter; //in pixel

    // Puzzle Grid
    public float _gridWidth = 3f;
    public float _gridHeight = 2f;

    public Material _sampleMaterial;

    List<Texture2D> _listOfTextiles;
    List<Piece> _gridPieces;
    public List<Piece> _puzzlePieces;

    Texture2D _mainTexture;

    float _overlapThreshold = 0.02f; //in meters
    int _differentZThreshold = 10;

    float _differentScale = 0.1f;
    int _numberOfScale = 10;
    Vector3 _standardPieceScale = Vector3.zero;


    float _differentAngle = 4;
    int _numberOfRotation = 80;
    Vector3 _standardRotation = Vector3.zero;

    Color _startPointsColor = new Color(255, 0, 0);
    Color _linePointsColor = new Color(255, 255, 0);
    Color _endPointsColor = new Color(0, 0, 255);

    List<Vector2> _startStopPoints;
    List<Vector2> _linePoints;

    Vector2 _firstGridPixelPosition = Vector2.zero;
    float _mainPuzzleTextureRatio = 1;

    public List<Pixel> _sketchedPixels;

    int _sketchedBrush = 5;
    public GameObject _puzzleDoneObject;

    //For sketch
    Vector2 _previous2DPoint = new Vector2(1000000, 1000000);


    //Status notification

    GameObject _canvasObject;
    public GameObject _statusObject;

    public Font _statusFont;

    public GameObject _startButtonObject;

    List<GameObject> _listOfGameObjects;

    [Header("Stage")]
    public bool isPuzzledDone = false;
    // Check sketch progress
    public bool isSketchStarted = false;
    public bool isInStartPoints = false;
    public bool isInLinePoints = false;
    public bool isInEndPoints = false;
    public bool isSketchingOnTrack = false;
    public bool isSketchDoneSucessfully = false;

    // Start is called before the first frame update
    void Start()
    {
        if (_startButtonTexture != null)
        {
            Quaternion startButtonRotation = new Quaternion();
            startButtonRotation.eulerAngles = new Vector3(0, 0, 180);

            _startButtonObject = CreateCubeGameObject("Start Button",
                                                      new Vector3(0, 2.5f, 0),
                                                      startButtonRotation,
                                                      new Vector3(0.5f, 0.5f, 0.5f),
                                                      null,
                                                      _startButtonTexture,
                                                      Color.white);
        }
        else
        {
            Debug.LogWarning("Missing start button texture!", _startButtonTexture);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Init(int _textureID, float _prepareTime)
    {
        _listOfGameObjects = new List<GameObject>();

        if (_textureID == 1)
        {
            _mainPuzzleTexture = _puzzle1O;
            _drawPuzzleTexture = _puzzle1L;
            //_originalPuzzleTexture = _puzzle1L;
        }
        else
        {
            _mainPuzzleTexture = _puzzle2O;
            _drawPuzzleTexture = _puzzle2L;
            //_originalPuzzleTexture = _puzzle2L;
        }

        _originalPuzzleTexture = new Texture2D(_drawPuzzleTexture.width, _drawPuzzleTexture.height);
        _originalPuzzleTexture.SetPixels(_drawPuzzleTexture.GetPixels());
        _originalPuzzleTexture.Apply();


        /// Large Screen Object
        /// 

        if (_largeScreenObject != null)
        {
            _largeScreenWidth = ConvertMetersToPixels(_largeScreenObject.transform.localScale.x);
            _largeScreenHeight = ConvertMetersToPixels(_largeScreenObject.transform.localScale.y);
        }
        else
        {
            _largeScreenObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            _largeScreenObject.name = "Large Screen";

            _largeScreenWidth = ConvertMetersToPixels(7f);
            _largeScreenHeight = ConvertMetersToPixels(3f);

            //_largeScreenWidth = ConvertMetersToPixels(15f);
            //_largeScreenHeight = ConvertMetersToPixels(3f);

            _largeScreenObject.transform.position = new Vector3(0, 2.5f, 0);
            _largeScreenObject.transform.rotation = Quaternion.identity;
            _largeScreenObject.transform.localScale = new Vector3(ConvertPixelsToMeters(_largeScreenWidth), ConvertPixelsToMeters(_largeScreenHeight), 0.01f);


            //_largeScreenObject.AddComponent<MeshFilter>();
            //_largeScreenObject.AddComponent<MeshRenderer>();

            if (_screenMaterial != null)
            {
                _largeScreenObject.GetComponent<Renderer>().material = _screenMaterial;
            }
            else
            {
                Debug.LogWarning("Missing screen material!", _screenMaterial);
            }

            _largeScreenCenter = new Vector2(_largeScreenWidth / 2, _largeScreenHeight / 2);

            Debug.LogWarning("Large screen object is missed! A random object is generated.", _largeScreenObject);
        }


        if (_mainPuzzleTexture != null)
        {
            _marginSize = _largeScreenWidth * _percentageForMargins;

            Vector2 _mainTextureArea = new Vector2(0.4f * (_largeScreenWidth - 4 * _marginSize), _largeScreenHeight - 2 * _marginSize);

            _mainTextureArea = RescaleArea(_mainTextureArea, new Vector2(_mainPuzzleTexture.width, _mainPuzzleTexture.height));

            _mainPuzzleTextureRatio = _mainTextureArea.x / _mainPuzzleTexture.width;

            /// Sample Screen Area
            /// 

            Vector2 _sampleScreenArea = new Vector2(0.2f * (_largeScreenWidth - 4 * _marginSize), _largeScreenHeight - 2 * _marginSize);

            _sampleScreenArea = RescaleArea(_sampleScreenArea, new Vector2(_mainPuzzleTexture.width, _mainPuzzleTexture.height));


            _sampleScreenCenter = new Vector2(_sampleScreenArea.x / 2, _sampleScreenArea.y / 2);

            Quaternion sampleScreenRotation = new Quaternion();
            sampleScreenRotation.eulerAngles = new Vector3(0, 0, 180);

            GameObject sampleScreenObject = CreateCubeGameObject("Sample Screen",
                                                            new Vector3(_largeScreenObject.transform.position.x + ConvertPixelsToMeters(_largeScreenWidth * 0.5f -
                                                                                                                                        _marginSize -
                                                                                                                                        (int)_sampleScreenArea.x / 2),
                                                                        _largeScreenObject.transform.position.y,
                                                                        -0.0001f),
                                                            sampleScreenRotation,
                                                            new Vector3(ConvertPixelsToMeters(_sampleScreenArea.x),
                                                                        ConvertPixelsToMeters(_sampleScreenArea.y),
                                                                        0.01f),
                                                            null,
                                                            _mainPuzzleTexture,
                                                            Color.white);

            /// Puzzle Area
            /// 

            Vector2 puzzleArea = new Vector2(0.4f * (_largeScreenWidth - 4 * _marginSize), _largeScreenHeight - 2 * _marginSize);

            //puzzleArea = RescaleArea(puzzleArea, new Vector2(_mainPuzzleTexture.width, _mainPuzzleTexture.height));

            Quaternion puzzleAreaRotation = new Quaternion();
            sampleScreenRotation.eulerAngles = new Vector3(0, 0, 180);

            float middleMargin = (_largeScreenWidth - puzzleArea.x - _sampleScreenArea.x - _mainTextureArea.x - 4 * _marginSize) / 2;
            float rightPart = middleMargin + 2 * _marginSize + _sampleScreenArea.x;
            float shift = rightPart - _largeScreenWidth / 2;

            float puzzlePositionX = puzzleArea.x / 2 + shift;

            GameObject puzzleAreaObject = CreateCubeGameObject("Puzzle Area",
                                                                new Vector3(_largeScreenObject.transform.position.x - ConvertPixelsToMeters(puzzlePositionX),
                                                                            _largeScreenObject.transform.position.y,
                                                                            -0.0001f),
                                                                puzzleAreaRotation,
                                                                new Vector3(ConvertPixelsToMeters(puzzleArea.x),
                                                                            ConvertPixelsToMeters(puzzleArea.y),
                                                                            0.01f),
                                                                null,
                                                                null,
                                                                Color.white);

            /// Puzzle Grid Area
            /// 
            if (_gridPieceTexture != null)
            {
                Quaternion puzzleDoneRotation = new Quaternion();
                puzzleDoneRotation.eulerAngles = new Vector3(0, 0, 180);

                Vector3 puzzleDonePosition = Vector3.zero;

                puzzleDonePosition.x = _largeScreenObject.transform.position.x - ConvertPixelsToMeters(_largeScreenCenter.x - _marginSize - _mainTextureArea.x / 2);
                puzzleDonePosition.y = _largeScreenObject.transform.position.y + ConvertPixelsToMeters(_largeScreenCenter.y - _marginSize - _mainTextureArea.y / 2);
                puzzleDonePosition.z = -0.005f;

                _puzzleDoneObject = CreateCubeGameObject("Puzzle Done",
                                                         puzzleDonePosition,
                                                         puzzleDoneRotation,
                                                         new Vector3(ConvertPixelsToMeters(_mainTextureArea.x),
                                                                     ConvertPixelsToMeters(_mainTextureArea.y),
                                                                     0.01f),
                                                         null,
                                                         _drawPuzzleTexture,
                                                         Color.white);

                Destroy(_puzzleDoneObject.GetComponent<BoxCollider>());
                _puzzleDoneObject.AddComponent<MeshCollider>();

                _puzzleDoneObject.SetActive(false);

                _gridPieces = new List<Piece>();
                _puzzlePieces = new List<Piece>();

                GameObject gridMasterObject = new GameObject();
                gridMasterObject.name = "Puzzle Grid";

                GameObject puzzleMasterObject = new GameObject();
                puzzleMasterObject.name = "Puzzle Pieces";

                float _gridPieceWidth = _mainTextureArea.x / _x;
                float _gridPieceHeight = _mainTextureArea.y / _y;
                Vector2 _firstGridPosition = new Vector2(_largeScreenObject.transform.position.x - ConvertPixelsToMeters(_largeScreenCenter.x - _marginSize - _gridPieceWidth / 2),
                                                         _largeScreenObject.transform.position.y + ConvertPixelsToMeters(_largeScreenCenter.y - _marginSize - _gridPieceHeight / 2));

                _firstGridPixelPosition = new Vector2(_largeScreenObject.transform.position.x - ConvertPixelsToMeters(_largeScreenCenter.x - _marginSize),
                                                      _largeScreenObject.transform.position.y - ConvertPixelsToMeters(_largeScreenCenter.y - _marginSize));

                int puzzlePieceWidthScaledUp = (int)(_mainTextureArea.x / _x);
                int puzzlePieceHeightScaledUp = (int)(_mainTextureArea.y / _y);

                _overlapThreshold = ConvertPixelsToMeters(puzzlePieceHeightScaledUp / 5);

                int puzzlePieceWidth = (int)(_mainPuzzleTexture.width / _x);
                int puzzlePieceHeight = (int)(_mainPuzzleTexture.height / _y);

                _startStopPoints = new List<Vector2>();
                _linePoints = new List<Vector2>();

                for (int i = 0; i < _x; i++)
                {
                    for (int j = 0; j < _y; j++)
                    {
                        Quaternion gridPieceRotation = new Quaternion();
                        gridPieceRotation.eulerAngles = new Vector3(0, 0, 180);

                        Vector3 gridPiecePosition = new Vector3(_firstGridPosition.x + ConvertPixelsToMeters(i * _gridPieceWidth),
                                                                _firstGridPosition.y - ConvertPixelsToMeters(j * _gridPieceHeight),
                                                                -0.0001f);

                        GameObject gridPieceObject = CreateCubeGameObject("Grid Piece " + (i * _x + _y - j).ToString(),
                                                                        gridPiecePosition,
                                                                        gridPieceRotation,
                                                                        new Vector3(ConvertPixelsToMeters(_gridPieceWidth),
                                                                                    ConvertPixelsToMeters(_gridPieceHeight),
                                                                                    0.01f),
                                                                        null,
                                                                        _gridPieceTexture,
                                                                        Color.white);

                        //Vector2 check = ConvertPositionToPixelPosition(new Vector2(gridPiecePosition.x, gridPiecePosition.y));

                        //if (check.x < 0 || check.y < 0 || check.x > _mainPuzzleTexture.width || check.y > _mainPuzzleTexture.height)
                        //{
                        //    Debug.Log("Exceed texture!");
                        //}

                        gridPieceObject.transform.parent = gridMasterObject.transform;

                        Piece gridPiece = new Piece(gridPieceObject, i.ToString() + (_y - j - 1).ToString());
                        _gridPieces.Add(gridPiece);



                        /// Generate Puzzle Pieces
                        /// 

                        Texture2D puzzleTextureOrginal = new Texture2D(puzzlePieceWidth, puzzlePieceHeight);
                        Texture2D puzzleTextureHighlighted = new Texture2D(puzzlePieceWidth, puzzlePieceHeight);

                        for (int x = 0; x < puzzlePieceWidth; x++)
                        {
                            for (int y = 0; y < puzzlePieceHeight; y++)
                            {
                                Color color = _mainPuzzleTexture.GetPixel(x + i * puzzlePieceWidth, y + j * puzzlePieceHeight);
                                puzzleTextureOrginal.SetPixel(x, y, color);

                                if ((x < 15) || (x > puzzlePieceWidth - 15) || (y < 15) || (y > puzzlePieceHeight - 15))
                                {
                                    puzzleTextureHighlighted.SetPixel(x, y, Color.black);
                                }
                                else
                                    puzzleTextureHighlighted.SetPixel(x, y, color);


                                // Get Start, Stop, and Line Points
                                if (color.r == 1f && color.g == 0f && color.b == 0f)
                                {
                                    _startStopPoints.Add(new Vector2(x + i * puzzlePieceWidth, y + j * puzzlePieceHeight));
                                }
                                else if (color.r == 1f && color.g == 1f && color.b == 0f)
                                {
                                    _linePoints.Add(new Vector2(x + i * puzzlePieceWidth, y + j * puzzlePieceHeight));
                                }
                            }
                        }

                        puzzleTextureOrginal.Apply();
                        puzzleTextureHighlighted.Apply();

                        Quaternion puzzlePieceRotation = new Quaternion();


                        _standardRotation = new Vector3(0, 0, 180);

                        float zAngle = ((int)(UnityEngine.Random.Range(0, _numberOfRotation))) * _differentAngle;

                        puzzlePieceRotation.eulerAngles = new Vector3(0, 0, zAngle);

                        //puzzlePieceRotation.eulerAngles = new Vector3(0, 0, StandardizeRotationAngle(UnityEngine.Random.Range(0, 360), _differentZThreshold));

                        Vector3 puzzlePiecePosition = Vector3.zero;

                        float gridPieceWidthInMeter = ConvertPixelsToMeters(_gridPieceWidth);
                        float gridPieceHeightInMeter = ConvertPixelsToMeters(_gridPieceHeight);

                        _standardPieceScale = new Vector3(gridPieceWidthInMeter, gridPieceHeightInMeter, 0.01f);

                        float ratio = ((int)(((int)(UnityEngine.Random.Range(1, _numberOfScale))) / 2)) * _differentScale;

                        if (UnityEngine.Random.Range(-1, 1) < 0)
                        {
                            ratio *= (-1);
                        }

                        Vector3 puzzlePieceScale = Vector3.zero;
                        puzzlePieceScale.x = _standardPieceScale.x * (1 + ratio);
                        puzzlePieceScale.y = _standardPieceScale.y * (1 + ratio);
                        puzzlePieceScale.z = _standardPieceScale.z;

                        do
                        {
                            bool flag = false;

                            float x = UnityEngine.Random.Range(puzzleAreaObject.transform.position.x - puzzleAreaObject.transform.localScale.x / 2 + puzzlePieceScale.x / 2,
                                                    puzzleAreaObject.transform.position.x + puzzleAreaObject.transform.localScale.x / 2 - puzzlePieceScale.x / 2);
                            float y = UnityEngine.Random.Range(puzzleAreaObject.transform.position.y - puzzleAreaObject.transform.localScale.y / 2 + puzzlePieceScale.y / 2,
                                                    puzzleAreaObject.transform.position.y + puzzleAreaObject.transform.localScale.y / 2 - puzzlePieceScale.y / 2);

                            float z = UnityEngine.Random.Range(-0.005f, -0.0015f);

                            for (int id = 0; id < _puzzlePieces.Count; id++)
                            {
                                if (_puzzlePieces[id].GameObject.transform.position.z == z)
                                {
                                    flag = true;
                                }
                            }

                            if (!flag)
                            {
                                puzzlePiecePosition = new Vector3(x, y, z);

                                break;
                            }
                        }
                        while (true);

                        GameObject puzzlePieceObject = CreateCubeGameObject("Puzzle Piece " + (i * _x + j + 1).ToString(),
                                                                            puzzlePiecePosition,
                                                                            puzzlePieceRotation,
                                                                            puzzlePieceScale,
                                                                            null,
                                                                            puzzleTextureOrginal,
                                                                            Color.white);

                        Destroy(puzzlePieceObject.GetComponent<BoxCollider>());
                        puzzlePieceObject.AddComponent<MeshCollider>();

                        puzzlePieceObject.transform.parent = puzzleMasterObject.transform;

                        Piece puzzlePiece = new Piece(puzzlePieceObject, i.ToString() + j.ToString(), false, puzzleTextureOrginal, puzzleTextureHighlighted, (1 + ratio));
                        _puzzlePieces.Add(puzzlePiece);
                    }
                }

                _puzzlePieces = SortPieces(_puzzlePieces);

                //isInit = true;
                Debug.Log("Init finished!");

                //For testing
                /*_puzzleDoneObject.SetActive(true);
                foreach (var grid in _gridPieces)
                {
                    foreach (var puzzle in _puzzlePieces)
                    {
                        if (grid.Name == puzzle.Name)
                        {
                            puzzle.GameObject.transform.position = new Vector3(grid.GameObject.transform.position.x,
                                                                               grid.GameObject.transform.position.y,
                                                                               puzzle.GameObject.transform.position.z);
                            puzzle.GameObject.transform.rotation = grid.GameObject.transform.rotation;
                            puzzle.GameObject.transform.localScale = grid.GameObject.transform.localScale;

                            puzzle.GameObject.SetActive(false);
                        }
                    }
                }*/


                _listOfGameObjects.Add(sampleScreenObject);
                _listOfGameObjects.Add(puzzleMasterObject);
                _listOfGameObjects.Add(gridMasterObject);
                _listOfGameObjects.Add(puzzleAreaObject);
            }
            else
            {
                Debug.LogWarning("Missing grid piece texture!", _gridPieceTexture);
            }
        }
        else
        {
            Debug.LogWarning("Missing puzzle texture!", _mainPuzzleTexture);
        }

        _canvasObject = new GameObject();
        _canvasObject.AddComponent<Canvas>();
        _canvasObject.AddComponent<CanvasScaler>();
        _canvasObject.GetComponent<CanvasScaler>().dynamicPixelsPerUnit = 75;
        _canvasObject.GetComponent<RectTransform>().position = new Vector3(_largeScreenObject.transform.position.x,
                                                                           _largeScreenObject.transform.position.y,
                                                                           _largeScreenObject.transform.position.z - 0.0125f);

        float canvasScaleXY = _largeScreenObject.transform.localScale.x / 100 > _largeScreenObject.transform.localScale.y / 100 ? _largeScreenObject.transform.localScale.y / 100 : _largeScreenObject.transform.localScale.x / 100;
        _canvasObject.GetComponent<RectTransform>().localScale = new Vector3(canvasScaleXY,
                                                                             canvasScaleXY,
                                                                             _largeScreenObject.transform.localScale.z / 100);
        _canvasObject.gameObject.name = "Status Canvas";
        //_canvasObject.gameObject.transform.position = _largeScreenObject.transform.position;

        _statusObject = new GameObject();
        _statusObject.gameObject.transform.parent = _canvasObject.transform;
        _statusObject.gameObject.name = "Status Text";
        _statusObject.AddComponent<Text>();
        _statusObject.GetComponent<Text>().text = "Please start to sketch!";
        _statusObject.GetComponent<Text>().font = _statusFont;
        _statusObject.GetComponent<Text>().color = Color.black;
        _statusObject.GetComponent<Text>().fontSize = 4;
        _statusObject.GetComponent<Text>().alignment = TextAnchor.LowerCenter;
        _statusObject.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        _statusObject.GetComponent<RectTransform>().localPosition = new Vector3(0, 0.75f, 0);

        _listOfGameObjects.Add(_largeScreenObject);

        if (_prepareTime > 0)
        {
            foreach (var e in _listOfGameObjects)
            {
                e.SetActive(false);
            }
        }

        _isInit = true;
    }

    public void CheckSketch()
    {
        if (CheckPointPixelColor(_startPointsColor, _linePointsColor, _endPointsColor))
        {
            if (!isSketchDoneSucessfully)
            {
                _statusObject.GetComponent<Text>().text = "Sketching...";
                _statusObject.GetComponent<Text>().color = Color.black;
            }
            else
            {
                Debug.Log("Sketch is successfully done!");
                isSketchDoneSucessfully = true;
                _statusObject.GetComponent<Text>().text = "Sketch is successfully done!";
            }
        }
        else
        {
            isSketchingOnTrack = false;
            _statusObject.GetComponent<Text>().color = Color.red;
            _statusObject.GetComponent<Text>().text = "Please keep sketching on track!";
        }
    }
    bool CheckPointPixelColor(Color startColor, Color lineColor, Color endColor)
    {
        bool ret = true;

        RaycastHit hit;

        if (!Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
            return false;

        Renderer rend = hit.transform.GetComponent<Renderer>();
        MeshCollider mesh = hit.collider as MeshCollider;

        if (rend == null || rend.sharedMaterial == null || rend.sharedMaterial.mainTexture == null || mesh == null)
            return false;

        if (!hit.transform.name.Contains("Puzzle Done"))
            return false;

        Texture2D texture2D = rend.material.mainTexture as Texture2D;

        Vector2 pixelUV = hit.textureCoord;

        pixelUV.x *= texture2D.width;
        pixelUV.y *= texture2D.height;

        Color32 c;
        c = _originalPuzzleTexture.GetPixel((int)pixelUV.x, (int)pixelUV.y);//texture2D.GetPixel((int)pixelUV.x, (int)pixelUV.y);

        if (Mathf.Abs(c.r - startColor.r) < 20 &&
           Mathf.Abs(c.g - startColor.g) < 20 &&
           Mathf.Abs(c.b - startColor.b) < 20)
        {
            isInStartPoints = true;
            isInLinePoints = false;
            isInEndPoints = false;

            isSketchingOnTrack = true;
        }
        else if (isInStartPoints &&
                 isInLinePoints &&
                Mathf.Abs(c.r - endColor.r) < 20 &&
                Mathf.Abs(c.g - endColor.g) < 20 &&
                Mathf.Abs(c.b - endColor.b) < 20)
        {
            isInStartPoints = true;
            isInLinePoints = true;
            isInEndPoints = true;

            isSketchingOnTrack = true;
            isSketchDoneSucessfully = true;
        }
        else if (isInStartPoints &&
                 Mathf.Abs(c.r - lineColor.r) < 20 &&
                 Mathf.Abs(c.g - lineColor.g) < 20 &&
                 Mathf.Abs(c.b - lineColor.b) < 20)
        {
            isInStartPoints = true;
            isInLinePoints = true;
            isInEndPoints = false;

            isSketchingOnTrack = true;
        }
        else
        {
            isSketchingOnTrack = false;
            ret = false;
        }

        hit.transform.GetComponent<Renderer>().material.mainTexture = BrushSketchLines(texture2D, (int)pixelUV.x, (int)pixelUV.y);

        Vector2 difUV = pixelUV - _previous2DPoint;

        int signX = 1;
        int signY = 1;

        int difX = (int)pixelUV.x - (int)_previous2DPoint.x;

        if ((int)pixelUV.x < _previous2DPoint.x)
        {
            signX = (-1);
        }

        int difY = (int)pixelUV.y - (int)_previous2DPoint.y;

        if ((int)pixelUV.y < _previous2DPoint.y)
        {
            signY = (-1);
        }

        difX *= signX;
        difY *= signY;

        float difXY = (float)difX / difY;
        float difYX = (float)difY / difX;

        if (difUV.magnitude > 0 &&
            difUV.magnitude < 75)
        {
            for (int i = 0, j = 0; i < difX - 1 || j < difY - 1;)
            {
                if (difX > difY)
                {
                    i += 1;
                    j = (int)(difYX * i);
                }
                else if (difX == difY)
                {
                    i++;
                    j++;
                }
                else
                {
                    j += 1;
                    i = (int)(difXY * j);
                }

                BrushSketchLines(texture2D, (int)_previous2DPoint.x + i * signX, (int)_previous2DPoint.y + j * signY);
            }
        }
        //else
        //{
        //    if ((_previous2DPoint - pixelUV).magnitude > 4)
        //        Debug.Log("_previous2DPoint: " +
        //                  _previous2DPoint.x +
        //                  "," +
        //                  _previous2DPoint.y +
        //                  "; pixelUV: " +
        //                  pixelUV.x +
        //                  "," +
        //                  pixelUV.y +
        //                  ", maginitude:" +
        //                  (_previous2DPoint - pixelUV).magnitude +
        //                  "," +
        //                  difX +
        //                  "," +
        //                  difY);
        //}

        _previous2DPoint = new Vector2((int)pixelUV.x, (int)pixelUV.y);

        return ret;
    }

    private Texture2D BrushSketchLines(Texture2D tex, int x, int y)
    {
        for (int i = 0; i < _sketchedBrush; i++)
        {
            for (int j = 0; j < _sketchedBrush; j++)
            {
                for (int sign = 0; sign < 4; sign++)
                {
                    int a, b;
                    a = x;
                    b = y;

                    if (sign == 0)
                    {
                        a += i;
                        b += j;
                    }
                    else if (sign == 1)
                    {
                        a -= i;
                        b += j;
                    }
                    else if (sign == 2)
                    {
                        a += i;
                        b -= j;
                    }
                    else if (sign == 3)
                    {
                        a -= i;
                        b -= j;
                    }

                    if (a >= tex.width)
                        a = tex.width - 1;

                    if (b >= tex.height)
                        b = tex.height - 1;

                    if (a < 0)
                        a = 0;

                    if (b < 0)
                        b = 0;

                    float distance = (new Vector2(a, b) - new Vector2(x, y)).magnitude;

                    if (distance <= _sketchedBrush - 1)
                    {
                        if (tex.GetPixel(a, b) != Color.black)
                        {
                            Pixel p = new Pixel(a, b, tex.GetPixel(a, b), Color.black);
                            tex.SetPixel(a, b, Color.black);

                            _sketchedPixels.Add(p);
                        }
                    }
                }
            }
        }

        tex.Apply();

        return tex;
    }

    public bool CheckPuzzlesDone()
    {
        foreach (var gridPiece in _gridPieces)
        {
            foreach (var puzzlePiece in _puzzlePieces)
            {
                if (puzzlePiece.Name == gridPiece.Name && (new Vector3(puzzlePiece.GameObject.transform.position.x - gridPiece.GameObject.transform.position.x,
                                                                      puzzlePiece.GameObject.transform.position.y - gridPiece.GameObject.transform.position.y,
                                                                      0)).magnitude > 0.02f)
                {
                    return false;
                }

                //if (puzzlePiece.Name == gridPiece.Name && (new Vector3(puzzlePiece.GameObject.transform.position.x - gridPiece.GameObject.transform.position.x,
                //                                                      puzzlePiece.GameObject.transform.position.y - gridPiece.GameObject.transform.position.y,
                //                                                      0)).magnitude < 0.02f)
                //{
                //    Debug.Log(puzzlePiece.Name);
                //}
            }
        }

        return true;
    }

    public void UpdatePiecePosition(Piece _selectedPiece)
    {
        RaycastHit hitInfo = new RaycastHit();

        bool hit = Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo);

        bool flag = false;

        Piece gridPieceOverlapped = null;

        foreach (var gridPiece in _gridPieces)
        {
            if ((new Vector3(hitInfo.point.x, hitInfo.point.y, _selectedPiece.GameObject.transform.position.z) - gridPiece.GameObject.transform.position).magnitude < 0.02f)
            {
                flag = true;
                gridPieceOverlapped = gridPiece;

                break;
            }
        }

        if (!flag)
        {
            _selectedPiece.GameObject.transform.position = new Vector3(hitInfo.point.x, hitInfo.point.y, _selectedPiece.GameObject.transform.position.z);
        }
        else
            _selectedPiece.GameObject.transform.position = new Vector3(gridPieceOverlapped.GameObject.transform.position.x,
                                                                        gridPieceOverlapped.GameObject.transform.position.y,
                                                                        _selectedPiece.GameObject.transform.position.z);
    }

    public void HighlightGridPiece()
    {

        foreach (var gridPiece in _gridPieces)
        {
            bool flag = false;

            foreach (var piece in _puzzlePieces)
            {
                if ((piece.GameObject.transform.position - gridPiece.GameObject.transform.position).magnitude < _overlapThreshold)
                {
                    flag = true;
                    break;
                }
            }

            if (flag)
                gridPiece.GameObject.GetComponent<Renderer>().material.color = Color.yellow;
            else
                gridPiece.GameObject.GetComponent<Renderer>().material.color = Color.white;
        }
    }


    Vector2 ConvertPositionToPixelPosition(Vector2 position)
    {
        Vector2 ret = Vector2.zero;

        ret.x = (int)(ConvertMetersToPixels(position.x - _firstGridPixelPosition.x) / _mainPuzzleTextureRatio);
        ret.y = (int)(ConvertMetersToPixels(position.y - _firstGridPixelPosition.y) / _mainPuzzleTextureRatio);

        if (ret.x < 0)
            ret.x *= (-1);

        if (ret.y < 0)
            ret.y *= (-1);

        return ret;
    }

    bool InStartPoints(Vector2 point)
    {
        foreach (var p in _startStopPoints)
        {
            if (p.x == point.x && p.y == point.y)
            {
                return true;
            }
        }

        return false;
    }

    bool InLinePoints(Vector2 point)
    {
        foreach (var p in _linePoints)
        {
            if (p.x == point.x && p.y == point.y)
            {
                return true;
            }
        }

        return false;
    }

    private void ResetTexture()
    {

        if (_puzzleDoneObject != null)
        {
            Texture2D texture2D = _puzzleDoneObject.gameObject.GetComponent<Renderer>().material.mainTexture as Texture2D;
            foreach (var p in _sketchedPixels)
            {
                texture2D.SetPixel(p.X, p.Y, p.PreviousColor);
            }

            texture2D.Apply();
            _sketchedPixels.Clear();
            _puzzleDoneObject.gameObject.GetComponent<Renderer>().material.mainTexture = texture2D;
        }
    }

    public List<Piece> SortPieces(List<Piece> list)
    {
        for (int i = 0; i < list.Count - 1; i++)
        {
            for (int j = i + 1; j < list.Count - 1; j++)
            {
                if (list[i].GameObject.transform.position.z > list[j].GameObject.transform.position.z)
                {
                    Piece tmp = list[i];
                    list[i] = list[j];
                    list[j] = tmp;
                }
            }
        }

        return list;
    }

    float ConvertPixelsToMeters(float pixels)
    {
        return pixels / _coefficientCoverterPixelToM;
    }

    float ConvertMetersToPixels(float meters)
    {
        return meters * _coefficientCoverterPixelToM;
    }

    GameObject CreateCubeGameObject(string name, Vector3 position, Quaternion rotation, Vector3 localScale, Material material, Texture2D texture, Color color)
    {
        GameObject newGameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        newGameObject.name = name;

        newGameObject.transform.position = position;
        newGameObject.transform.rotation = rotation;
        newGameObject.transform.localScale = localScale;

        //newGameObject.AddComponent<MeshFilter>();
        //newGameObject.AddComponent<MeshRenderer>();

        if (material != null)
            newGameObject.GetComponent<Renderer>().material = material;
        else if (texture != null)
            newGameObject.GetComponent<Renderer>().material.mainTexture = texture;
        else
            newGameObject.GetComponent<Renderer>().material.color = color;

        return newGameObject;
    }

    Vector2 RescaleArea(Vector2 orginal, Vector2 scale)
    {
        if (orginal.x / scale.x > orginal.y / scale.y)
        {
            orginal.x = scale.x * orginal.y / scale.y;
        }
        else
        {
            orginal.y = orginal.x / scale.x * scale.y;
        }

        return orginal;
    }

    int StandardizeRotationAngle(float angle, int standardThreshold = 10)
    {
        int coef = (int)(angle / 360);


        if (coef > 0)
        {
            angle -= coef * 360;
        }
        else if (coef < 0)
        {
            angle += coef * 360;
        }

        int dif = Mathf.RoundToInt(angle / _differentZThreshold);

        return dif * standardThreshold;
    }

    float NormalizeScaleLevel(float newScale, float orginialScale, float scaleSpace)
    {
        int time = (int)((newScale - orginialScale) / scaleSpace);

        return orginialScale + time * scaleSpace;
    }

    Vector3 StandardizeScale(Vector3 newScale, Vector3 standardScale, float scaleSpace)
    {
        int time = (int)((newScale.x - standardScale.x) / scaleSpace);

        return new Vector3(standardScale.x + scaleSpace * time, standardScale.y + scaleSpace * time, standardScale.z);
    }

    public Piece PuzzlePieceRotateLeft(Piece piece)
    {
        piece.GameObject.transform.Rotate(Vector3.forward, _differentAngle);

        return piece;
    }

    public Piece PuzzlePieceRotateRight(Piece piece)
    {
        piece.GameObject.transform.Rotate(Vector3.forward, (-1) * _differentAngle);

        return piece;
    }

    public Piece PuzzlePieceScaleUp(Piece piece)
    {
        piece.CurrentScaleLevel += _differentScale;
        piece.GameObject.transform.localScale = new Vector3(_standardPieceScale.x * piece.CurrentScaleLevel,
                                                            _standardPieceScale.y * piece.CurrentScaleLevel,
                                                            _standardPieceScale.z);

        return piece;
    }

    public Piece PuzzlePieceScaleDown(Piece piece)
    {
        piece.CurrentScaleLevel -= _differentScale;

        piece.GameObject.transform.localScale = new Vector3(_standardPieceScale.x * piece.CurrentScaleLevel,
                                                            _standardPieceScale.y * piece.CurrentScaleLevel,
                                                            _standardPieceScale.z);

        return piece;
    }

    Vector3 ScaleUp(Vector3 currentScale, float scaleLevel)
    {
        float x = currentScale.x + scaleLevel;
        float y = x * currentScale.y / currentScale.x;
        float z = currentScale.z;
        return new Vector3(x, y, z);
    }

    Vector3 ScaleDown(Vector3 currentScale, float scaleLevel)
    {
        float x = NormalizeScaleLevel(currentScale.x - scaleLevel, currentScale.x, scaleLevel);
        float y = x * currentScale.y / currentScale.x;
        float z = currentScale.z;
        return new Vector3(x, y, z);
    }

    public void SetObjectsActive(bool isActive)
    {
        foreach (var e in _listOfGameObjects)
        {
            e.SetActive(isActive);
        }
    }

    private void OnApplicationQuit()
    {
        ResetTexture();
    }
}