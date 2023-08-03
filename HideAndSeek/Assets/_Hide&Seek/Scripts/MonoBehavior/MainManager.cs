using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainManager : MonoBehaviour
{
    public static MainManager Instance { get; private set; }
    public event EventHandler OnMapChanged;
    public event EventHandler OnStateChanged;
    public event EventHandler OnSpawnEnemy;
    public event EventHandler OnReceiveEffect;
    public event EventHandler<OnPopupSpawnEventArgs> OnPopupSpawn;
    public class OnPopupSpawnEventArgs: EventArgs
    {
        public string text;
        public GameObject textPopup;
        public List<IPopup> popupList;
        public float timerExisted;
        public float popupSpeed;
    }

    //public bool enableEditMap;

    [SerializeField] private SOGameSettingsRefs _gameSettingsRefsSO;
    [SerializeField] private SOMapRefs[] _mapRefsSO;
    [SerializeField] private SOEnemyRefs[] _enemiesRefsSO;
    [SerializeField] private Transform _mapParent, _itemParent, _winningObject;
    [SerializeField] private Transform _player;
    [SerializeField] private Transform _cameraFollow;
    [SerializeField] private Camera _mainCamera;
    private CircleCollider2D _circleCollider2D;
    private bool _isEatItem;
    private bool _canMove;
    private Vector3 _moveDir;

    [SerializeField] private AudioSource _walkStepSound;
    [SerializeField] private GameObject _enemyGO, _wallGO, _spawnEffectGO, _itemGO, _textPopupGO;
    [SerializeField] private Button _tutorialButton, _playButton, _playAgainButton, _soundButton;
    private List<IState> _enemiesList;
    private List<IWall> _wallsList;
    private List<IState> _spawnEffect;
    private List<IItem> _itemsList;
    private List<IPopup> _popupList;

    private Vector3 _mouseWorldPosition;
    private Vector3 _cameraFollowWinningPos;
    private float _countdownToStartTimer;
    private float _gamePlayingTimer;
    private float _enemySpawnPhase;
    private float _itemSpawnPhase;
    private float _originalSize;
    private float _currentScore;
    private int _multiplerScore;
    private int _multiplerScoreDefault = 10;

    private bool _isMuteSound;
    private bool _isWinning;
    private bool _isReachWinningReq;
    private bool _isWarningEndPoint;
    private bool _isPlayWalkSound;
    private float _speedMultipler = 1f;
    private float _moveX, _moveY;
    private int _numEnemySpawn;
    private int _randomMap;

    [SerializeField] private SpriteRenderer _playerCurrentSprite;
    [SerializeField] private Image[] _playerSprites;
    [SerializeField] private Image _scoreItemImg;
    [SerializeField] private Image _speedItemImg;
    private enum State
    {
        MainMenu,
        WaitingToStart,
        CountDownToStart,
        GamePlaying,
        WarningEndPoint,
        GameOver,
        GameEnd,
    }

    private State _state;

    private void Awake()
    {
        if(Instance != null)
        {
            Destroy(this);
        } else
        {
            Instance = this;
        }
        _circleCollider2D = _player.GetComponent<CircleCollider2D>();
        _enemiesList = new List<IState>();
        _wallsList = new List<IWall>();
        _spawnEffect = new List<IState>();
        _itemsList = new List<IItem>();
        _popupList = new List<IPopup>();
        _originalSize = _mainCamera.orthographicSize;
    }

    private void Start()
    {
        SetUpDefault();
        _tutorialButton.onClick.AddListener(() =>
        {
            _state = State.WaitingToStart;
            OnStateChanged?.Invoke(this, EventArgs.Empty);
        });
        _playButton.onClick.AddListener(() =>
        {
            _state = State.CountDownToStart;
            OnStateChanged?.Invoke(this, EventArgs.Empty);
        });
        _playAgainButton.onClick.AddListener(() =>
        {
            SetUpDefault();
            _state = State.WaitingToStart;
            OnStateChanged?.Invoke(this, EventArgs.Empty);
        });
        _soundButton.onClick.AddListener(() =>
        {
            MuteSound();
        });
        NewEnemy.OnCatchPLayer += Enemy_OnCatchPLayer;
        StartCoroutine(MapGenerator());
        _state = State.MainMenu;
        OnStateChanged?.Invoke(this, EventArgs.Empty);
    }
    IEnumerator MapGenerator()
    {
        foreach(var wall in _wallsList)
        {
            wall.Deactivate();
        }
        yield return null;
        foreach (var wall in _mapRefsSO[_randomMap].wallsInfo)
        {
            if (wall.specialWall == SOMapRefs.WallInfo.SpecialWall.Normal)
            {
                AddWall(new Vector3(wall.posX, wall.posY), wall.wallRefsSO);
            }
            if (wall.specialWall == SOMapRefs.WallInfo.SpecialWall.Fake)
            {
                AddWall(new Vector3(wall.posX, wall.posY), wall.wallRefsSO, isFake: true);
            }
            if(wall.specialWall == SOMapRefs.WallInfo.SpecialWall.Finish)
            {
                AddWall(new Vector3(wall.posX, wall.posY), wall.wallRefsSO, isFinish: true);
            }
        }
        yield return null;
        OnMapChanged?.Invoke(this, EventArgs.Empty);
    }

    private void Enemy_OnCatchPLayer(object sender, System.EventArgs e)
    {
        _state = State.GameOver;
        _playerCurrentSprite.sprite = _playerSprites[8].sprite;
        OnStateChanged?.Invoke(this, EventArgs.Empty);

    }

    private void MuteSound()
    {
        if (!_isMuteSound)
        {
            AudioListener.volume = 0f;
            _isMuteSound = !_isMuteSound;
        }
        if(_isMuteSound )
        {
            AudioListener.volume = 1f;
            _isMuteSound = !_isMuteSound;
        }
    }

    private void SetUpDefault()
    {
        _randomMap = UnityEngine.Random.Range(0, _mapRefsSO.Length);
        _countdownToStartTimer = _gameSettingsRefsSO.countdownToStartTimerMax;
        _gamePlayingTimer = _gameSettingsRefsSO.gamePlayingTimerMax;
        _player.transform.position = _mapRefsSO[_randomMap].playerSpawnPos[UnityEngine.Random.Range(0, _mapRefsSO[_randomMap].playerSpawnPos.Length)];
        _winningObject.transform.position = _mapRefsSO[_randomMap].winningPos;
        NewEnemy._isCatchPlayer = false;
        _mainCamera.transform.position = Vector3.MoveTowards(_mainCamera.transform.position, _cameraFollow.position, 100);
        _mainCamera.orthographicSize = _gameSettingsRefsSO.cameraOriginalSize;
        _multiplerScore = _multiplerScoreDefault;
        _currentScore = 0;
        _enemySpawnPhase = PlayingTimerNomalized();
        _itemSpawnPhase = PlayingTimerNomalized();
        _isReachWinningReq = false;
        _isWinning = false;
        _isWarningEndPoint = false;
        _numEnemySpawn = 0;
        _cameraFollowWinningPos = new Vector3(_mapRefsSO[_randomMap].winningPos.x, _mapRefsSO[_randomMap].winningPos.y, -10f);
        for (int i = 0; i < _enemiesList.Count; i++)
        {
            _enemiesList[i].Deactivate();
        }
        for(int i = 0; i < _itemsList.Count; i++)
        {
            _itemsList[i].Deactivate();
        }
        for(int i = 0; i < _popupList.Count; i++)
        {
            _popupList[i].Deactive();
        }
        _playerCurrentSprite.sprite = _playerSprites[0].sprite;
        StartCoroutine(MapGenerator());
    }

    private void Update()
    {
        /*if(Input.GetKeyDown(KeyCode.Space) && enableEditMap)
        {
            StartCoroutine(MapGenerator());
        }*/
        switch(_state)
        {
            case State.WaitingToStart:
                break;
            case State.CountDownToStart:
                _countdownToStartTimer -= Time.deltaTime;
                if(_countdownToStartTimer < 0f)
                {
                    OnPopupSpawn?.Invoke(this, new OnPopupSpawnEventArgs
                    {
                        text = "Get " + _gameSettingsRefsSO.winningScore + " point to reach final",
                        textPopup = _textPopupGO,
                        popupList = _popupList,
                        timerExisted = 2f,
                        popupSpeed = 50f
                    });
                    _state = State.GamePlaying;
                    OnStateChanged?.Invoke(this, EventArgs.Empty);
                }
                break;
            case State.GamePlaying:
                _gamePlayingTimer -= Time.deltaTime;
                if(_gamePlayingTimer < 0f)
                {
                    _state = State.GameOver;
                    OnStateChanged?.Invoke(this,EventArgs.Empty);
                }
                UpdatePopupAction();
                Movement();
                SpriteChangedFacingDir();
                SpawnEnemy();
                SpawnItem();
                UpdateEnemiesAction();
                IncScoreByTimePlaying();
                FinishPointAppear();
                ReachFinalPos();
                break;
            case State.WarningEndPoint:
                _mainCamera.transform.position = Vector3.MoveTowards(_mainCamera.transform.position, _cameraFollowWinningPos, .1f);
                if(Vector3.Distance(_mainCamera.transform.position, _cameraFollowWinningPos) == 0f)
                {
                    if(!_isWarningEndPoint)
                    {
                        StartCoroutine(DisappearWallDelay(1f));
                    }
                }
                break;
            case State.GameOver:
                float targetSize = _originalSize * _gameSettingsRefsSO.cameraZoomFactor;
                if(targetSize != Camera.main.orthographicSize)
                {
                    _mainCamera.transform.position = Vector3.MoveTowards(_mainCamera.transform.position, _cameraFollow.position, 1f);
                    _mainCamera.orthographicSize = Mathf.Lerp(_mainCamera.orthographicSize, targetSize, Time.deltaTime * _gameSettingsRefsSO.cameraZoomSpeed);
                    if(targetSize == Mathf.Round(_mainCamera.orthographicSize))
                    {
                        _state = State.GameEnd;
                        OnStateChanged?.Invoke(this, EventArgs.Empty);
                        for (int i = 0; i < _enemiesList.Count; i++)
                        {
                            _enemiesList[i].Deactivate();
                        }
                        for(int i = 0; i < _itemsList.Count; i++)
                        {
                            _itemsList[i].Deactivate();
                        }
                    }
                }
                break;
            case State.GameEnd:
                break;
        }
    }

    IEnumerator DisappearWallDelay(float delayTime)
    {
        _isWarningEndPoint = !_isWarningEndPoint;
        yield return new WaitForSeconds(delayTime);
        for (int i = 0; i < _wallsList.Count; i++)
        {
            if (_wallsList[i].IsFinishWall())
            {
                _wallsList[i].Deactivate();
            }
        }
        yield return new WaitForSeconds(delayTime / 2);
        OnPopupSpawn?.Invoke(this, new OnPopupSpawnEventArgs
        {
            text = "Reach final point\nto win the game",
            textPopup = _textPopupGO,
            popupList = _popupList,
            timerExisted = 2f,
            popupSpeed = 50f
        });
        _state = State.GamePlaying;
    }

    private int _spriteRunFont = 2;
    private int _spriteRunBack = 4;
    private int _spriteRunSide = 6;

    private void SpriteChangedFacingDir()
    {
        if(_moveDir == Vector3.zero)
        {
            _playerCurrentSprite.sprite = _playerSprites[0].sprite;
        }
        if(_moveDir.x > 0 || _moveDir.x < 0)
        {
            _playerCurrentSprite.sprite = _playerSprites[_spriteRunSide].sprite;
            _spriteRunSide++;
            if(_moveDir.x > 0) _player.transform.localScale = Vector3.one;
            if (_moveDir.x < 0) _player.transform.localScale = new Vector3(-1, 1, 1);
            if (_spriteRunSide > 7) _spriteRunSide = 6;
        }
        if(_moveDir.y < 0f && _moveDir.x == 0f)
        {
            _playerCurrentSprite.sprite = _playerSprites[_spriteRunFont].sprite;
            _spriteRunFont++;
            if (_spriteRunFont > 3) _spriteRunFont = 2;
        }
        if(_moveDir.y > 0f && _moveDir.x == 0f)
        {
            _playerCurrentSprite.sprite = _playerSprites[_spriteRunBack].sprite;
            _spriteRunBack++;
            if (_spriteRunBack > 5) _spriteRunBack = 4;
        }
    }

    public float PlayingTimerNomalized()
    {
        return _gamePlayingTimer / _gameSettingsRefsSO.gamePlayingTimerMax;
    }

    public float GetPlayingTimer()
    {
        return _gamePlayingTimer;
    }
    public void IncScoreByTimePlaying()
    {
        _currentScore += Time.deltaTime * _multiplerScore;
    }
    public int GetCurrentScore()
    {
        return Mathf.RoundToInt(_currentScore);
    }

    private void FinishPointAppear()
    {
        if (!_isReachWinningReq)
        {
            if (GetCurrentScore() >= _gameSettingsRefsSO.winningScore)
            {
                _state = State.WarningEndPoint;
                OnStateChanged?.Invoke(this, EventArgs.Empty);
                _isReachWinningReq = true;
            }
        }
    }

    private void ReachFinalPos()
    {
        if(_isReachWinningReq)
        {
            _isWinning = Physics2D.CircleCast(_circleCollider2D.bounds.center, _circleCollider2D.radius, _moveDir, 0f, _gameSettingsRefsSO.winningLayerMask);
            if(_isWinning)
            {
                _state = State.GameOver;
                OnStateChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    private void SpawnEnemy()
    {
        if ( _enemySpawnPhase > PlayingTimerNomalized())
        {
            Vector3 closedPlayerPos = Vector3.zero;
            float dst = 100f;
            foreach(var spawnPos in _mapRefsSO[_randomMap].enemySpawnPos)
            {
                if(Vector3.Distance(spawnPos, _player.transform.position) < dst)
                {
                    dst = Vector3.Distance(spawnPos, _player.transform.position);
                    closedPlayerPos = spawnPos;
                }
            }
            _enemySpawnPhase -= _gameSettingsRefsSO.enemySpawnRate;
            if(_numEnemySpawn < 3)
            {
                StartCoroutine(SpawnWarning(2f, closedPlayerPos));
            }
        }
    }

    IEnumerator SpawnWarning(float timer, Vector3 spawnPos)
    {
        float time = 0f;
        while(time < timer)
        {
            AddSpawnWarningEffect(spawnPos);
            yield return new WaitForSeconds(.1f);
            foreach (var index in _spawnEffect)
            {
                index.Deactivate();
            }
            yield return new WaitForSeconds(.2f);
            time += 0.2f;
        }
        AddEnemy(spawnPos);
        _numEnemySpawn++;
        OnSpawnEnemy?.Invoke(this, EventArgs.Empty);
    }

    private void SpawnItem()
    {
        if(_itemSpawnPhase > PlayingTimerNomalized())
        {
            Vector3 pos = Vector3.zero;
            foreach(Vector3 spawnPos in _mapRefsSO[_randomMap].itemSpawnPos)
            {
                if(!(Physics2D.CircleCast(spawnPos, .5f, Vector2.zero, 0, 256)))
                {
                    pos = spawnPos;
                    break;
                }
            }
            if (pos == Vector3.zero)
            {
                return;
            }
            _itemSpawnPhase -= _gameSettingsRefsSO.itemSpawnRate;
            AddItem(pos, _mapRefsSO[_randomMap].itemRefsSOs[UnityEngine.Random.Range(0, _mapRefsSO[_randomMap].itemRefsSOs.Length)]);
        }
    }

    private void GetEffect(IItem item)
    {

        if (item.GetIntTypeEffect() == (int)SOItemRefs.ItemEffect.Speed)
        {
            StartCoroutine(SpeedEffect(item.GetTimeEffect()));
        }
        if (item.GetIntTypeEffect() == (int)SOItemRefs.ItemEffect.Score)
        {
            StartCoroutine(ScoreEffect(item.GetTimeEffect()));
        }
        OnPopupSpawn?.Invoke(this, new OnPopupSpawnEventArgs
        {
            text = "Get " + item.GetItemName() + " in " + item.GetTimeEffect() + "s",
            textPopup = _textPopupGO,
            popupList = _popupList,
            timerExisted = 1f,
            popupSpeed = 150f
        });
        OnReceiveEffect?.Invoke(this, EventArgs.Empty);
        item.Deactivate();
    }

    IEnumerator ScoreEffect(float timer)
    {
        _multiplerScore = _multiplerScoreDefault * 2;
        yield return new WaitForSeconds(timer);
        _multiplerScore = _multiplerScoreDefault;
    }

    IEnumerator SpeedEffect(float timer)
    {
        _speedMultipler = 1.5f;
        yield return new WaitForSeconds(timer);
        _speedMultipler = 1f;
    }

    private void Movement()
    {
        if(Input.GetMouseButton(0))
        {
            _mouseWorldPosition = GetMouseWorldPosition();
            if (Vector3.Distance(_mouseWorldPosition, _player.position) < .5f)
            {
                return;
            }
            _moveDir = (_mouseWorldPosition - _player.position).normalized;
        } else
        {
            _moveX = 0;
            _moveY = 0;
            if (Input.GetKey(KeyCode.UpArrow))
            {
                _moveY = 1f;
            }
            if (Input.GetKey(KeyCode.DownArrow))
            {
                _moveY = -1f;
            }
            if (Input.GetKey(KeyCode.RightArrow))
            {
                _moveX = 1f;
            }
            if (Input.GetKey(KeyCode.LeftArrow))
            {
                _moveX = -1f;
            }
            _moveDir = new Vector3(_moveX, _moveY).normalized;
        }
        _canMove = !Physics2D.CircleCast(_circleCollider2D.bounds.center, _circleCollider2D.radius, _moveDir, 0.05f, _gameSettingsRefsSO.unWalkableLayerMask);
        _isEatItem = Physics2D.CircleCast(_circleCollider2D.bounds.center, _circleCollider2D.radius, _moveDir, .1f, _gameSettingsRefsSO.itemLayerMask);
        if (_isEatItem)
        {
            RaycastHit2D hit = Physics2D.CircleCast(_circleCollider2D.bounds.center, _circleCollider2D.radius, _moveDir, .1f, _gameSettingsRefsSO.itemLayerMask);
            foreach (var item in _itemsList)
            {
                if(item.GetGameObject().GetInstanceID() == hit.collider.gameObject.GetInstanceID())
                {
                    GetEffect(item);
                }

            }
        }
        if (!_canMove)
        {
            Vector3 moveDirX = new Vector3(_moveDir.x, 0f, 0f).normalized;
            _canMove = _moveDir.x != 0 && !Physics2D.CircleCast(_circleCollider2D.bounds.center, _circleCollider2D.radius, moveDirX, 0.05f, _gameSettingsRefsSO.unWalkableLayerMask);
            if (_canMove)
            {
                _moveDir = moveDirX;
            }
            else
            {
                Vector3 moveDirY = new Vector3(0, _moveDir.y, 0f).normalized;
                _canMove = _moveDir.y != 0 && !Physics2D.CircleCast(_circleCollider2D.bounds.center, _circleCollider2D.radius, moveDirY, 0.05f, _gameSettingsRefsSO.unWalkableLayerMask);
                if (_canMove)
                {
                    _moveDir = moveDirY;
                }
            }
        }
        if(_canMove)
        {
            _player.position += _moveDir * _gameSettingsRefsSO.playerSpeed * _speedMultipler * Time.deltaTime;
        }
        //_angle = Mathf.Atan2(_moveDir.x, _moveDir.y) * Mathf.Rad2Deg ;
        //if(_moveDir != Vector3.zero) _player.rotation = Quaternion.Euler(0f, 0f, -_angle);
        if(_moveDir != Vector3.zero && !_isPlayWalkSound)
        {
            StartCoroutine(DelayPlaySound(.2f));
        }
        Vector3 velocity = (_cameraFollow.position - _mainCamera.transform.position) * 3;
        _mainCamera.transform.position = Vector3.SmoothDamp(_mainCamera.transform.position, _cameraFollow.position, ref velocity, 1f, Time.deltaTime);
    }

    IEnumerator DelayPlaySound(float delayTime)
    {
        _isPlayWalkSound = !_isPlayWalkSound;
        _walkStepSound.Play();
        yield return new WaitForSeconds(delayTime);
        _isPlayWalkSound = !_isPlayWalkSound;
    }

    private Vector3 GetMouseWorldPosition()
    {
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        worldPosition.z = 0f;
        return worldPosition;
    }

    public bool FinalPosAppeared()
    {
        return _isWarningEndPoint;
    }
    public bool IsMainMenuState()
    {
        return _state == State.MainMenu;
    }
    public bool IsWaitingToStartState()
    {
        return _state == State.WaitingToStart;
    }

    public bool IsCountdownToStartState()
    {
        return _state == State.CountDownToStart;
    }
    public bool IsGamePlayingState()
    {
        return _state == State.GamePlaying;
    }
    public bool IsWarningEndPointState()
    {
        return _state == State.WarningEndPoint;
    }
    public bool IsGameOverState()
    {
        return _state == State.GameOver;
    }
    public bool IsGameEndState()
    {
        return _state == State.GameEnd;
    }

    public bool IsWinning()
    {
        return _isWinning;
    }

    public SOGameSettingsRefs GetGameSettings()
    {
        return _gameSettingsRefsSO;
    }
    private void UpdateEnemiesAction()
    {
        for(int i = 0; i < _enemiesList.Count; i++)
        {
            if (_enemiesList[i].GetState())
            {
                _enemiesList[i].Act();
            }
        }
    }

    private void UpdatePopupAction()
    {
        for(int i = 0; i < _popupList.Count;i++)
        {
            if (_popupList[i].GetActiveState())
            {
                _popupList[i].Act();
            }
        }
    }

    private void AddEnemy(Vector3 pos)
    {
        bool isAdded = false;
        for (int i = 0; i < _enemiesList.Count; i++)
        {
            if (_enemiesList[i].GetState() == false)
            {
                _enemiesList[i].Activate(pos, Vector3.zero);
                isAdded = true;
                break;
            }
        }
        if(!isAdded)
        {
            _enemiesList.Add(Factory.CreateEnemy(GameObject.Instantiate(_enemyGO), _player, _enemiesRefsSO[UnityEngine.Random.Range(0, _enemiesRefsSO.Length)], pos));
        }
    }

    private void AddWall(Vector3 pos, WallRefsSO wallRefsSO, bool isFinish = false, bool isFake = false)
    {
        bool isAdded = false;
        for (int i = 0; i < _wallsList.Count; i++)
        {
            if (_wallsList[i].GetActivateState() == false && _wallsList[i].IsFinishWall() == isFinish && _wallsList[i].IsFakeWall() == isFake)
            {
                _wallsList[i].Activate(pos, wallRefsSO);
                isAdded = true;
                break;
            }
        }
        if(!isAdded)
        {
            _wallsList.Add(Factory.CreateWall(GameObject.Instantiate(_wallGO, _mapParent), pos, wallRefsSO, isFinish, isFake));
        }
    }

    private void AddItem(Vector3 pos, SOItemRefs itemRefsSO)
    {
        bool isAdded = false;
        for (int i = 0; i < _itemsList.Count; i++)
        {
            if (_itemsList[i].GetActivateState() == false)
            {
                _itemsList[i].Activate(pos, itemRefsSO);
                if (itemRefsSO.effect == SOItemRefs.ItemEffect.Speed) _itemsList[i].SettingSprite(_speedItemImg.sprite);
                if (itemRefsSO.effect == SOItemRefs.ItemEffect.Score) _itemsList[i].SettingSprite(_scoreItemImg.sprite);
                isAdded = true;
                break;
            }
        }
        if (!isAdded)
        {
            _itemsList.Add(Factory.CreateItem(GameObject.Instantiate(_itemGO, _itemParent), pos, itemRefsSO));
            if (itemRefsSO.effect == SOItemRefs.ItemEffect.Speed) _itemsList[_itemsList.Count - 1].SettingSprite(_speedItemImg.sprite);
            if (itemRefsSO.effect == SOItemRefs.ItemEffect.Score) _itemsList[_itemsList.Count - 1].SettingSprite(_scoreItemImg.sprite);
        }
    }

    private void AddSpawnWarningEffect(Vector3 pos)
    {
        bool isAdded = false;
        for (int i = 0; i < _spawnEffect.Count; i++)
        {
            if (_spawnEffect[i].GetState() == false)
            {
                _spawnEffect[i].Activate(pos,Vector3.zero);
                isAdded = true;
                break;
            }
        }
        if (!isAdded)
        {
            _spawnEffect.Add(Factory.CreateSpawnWarning(GameObject.Instantiate(_spawnEffectGO), pos));
        }
    }
}
