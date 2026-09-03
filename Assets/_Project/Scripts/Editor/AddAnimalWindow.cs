using System;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using ZooWorld.Animals;
using ZooWorld.Animals.Definitions;
using ZooWorld.Core.Animals;
using ZooWorld.Spawning;
using Object = UnityEngine.Object;

namespace ZooWorld.Editor
{
    public sealed class AddAnimalWindow : EditorWindow
    {
        private const string AnimalsFolder = "Assets/_Project/Data/Animals";
        private const string MovementFolder = "Assets/_Project/Data/Movement";

        [SerializeField] private string _speciesId = "";
        [SerializeField] private FoodRole _foodRole;
        [SerializeField] private AnimalBehaviour _prefab;
        [SerializeField] private string _movementTypeName;
        [SerializeField] private string _movementJson;
        [SerializeField] private bool _addToSpawn = true;
        [SerializeField] private SpawnSettings _spawnSettings;

        private Type[] _movementTypes;
        private string[] _movementNames;
        private int _movementIndex;
        private MovementDefinition _movementDraft;
        private SerializedObject _movementProperties;
        private string _message;
        private MessageType _messageType;
        private Vector2 _scroll;

        [MenuItem("Zoo World/Add Animal")]
        private static void Open()
        {
            var window = GetWindow<AddAnimalWindow>("Add Animal");
            window.minSize = new Vector2(360f, 420f);
        }

        private void OnEnable()
        {
            if (_spawnSettings == null)
                _spawnSettings = AssetDatabase.LoadAssetAtPath<SpawnSettings>("Assets/_Project/Data/SpawnSettings.asset");

            _movementTypes = TypeCache.GetTypesDerivedFrom<MovementDefinition>()
                .Where(type => !type.IsAbstract && !type.ContainsGenericParameters)
                .OrderBy(type => type.FullName, StringComparer.Ordinal)
                .ToArray();
            _movementNames = _movementTypes.Select(type => ObjectNames.NicifyVariableName(type.Name)).ToArray();

            if (_movementTypes.Length == 0)
                return;

            int index = Array.FindIndex(_movementTypes, type => type.AssemblyQualifiedName == _movementTypeName);

            if (index < 0)
            {
                index = 0;
                _movementJson = null;
            }

            CreateMovementDraft(index);
        }

        private void OnDisable()
        {
            ReleaseMovementDraft();
        }

        private void OnGUI()
        {
            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            EditorGUILayout.LabelField("Новое животное", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            using (new EditorGUI.DisabledScope(EditorApplication.isPlayingOrWillChangePlaymode))
            {
                EditorGUI.BeginChangeCheck();
                _speciesId = EditorGUILayout.TextField("Species Id", _speciesId);
                _foodRole = (FoodRole)EditorGUILayout.EnumPopup("Food Role", _foodRole);
                _prefab = (AnimalBehaviour)EditorGUILayout.ObjectField("Prefab", _prefab, typeof(AnimalBehaviour), false);

                EditorGUILayout.Space();
                DrawMovement();

                EditorGUILayout.Space();
                _addToSpawn = EditorGUILayout.Toggle("Добавить в спавн", _addToSpawn);

                if (_addToSpawn)
                    _spawnSettings = (SpawnSettings)EditorGUILayout.ObjectField("Spawn Settings", _spawnSettings, typeof(SpawnSettings), false);

                if (EditorGUI.EndChangeCheck())
                    _message = null;

                EditorGUILayout.HelpBox("Species Id: строчные латинские буквы, цифры и _. Например, rabbit. " +
                    "Выбери готовый префаб с AnimalBehaviour на корне.", MessageType.Info);

                using (new EditorGUI.DisabledScope(_movementDraft == null))
                {
                    if (GUILayout.Button("Создать животное", GUILayout.Height(30f)))
                        CreateAnimal();
                }
            }

            if (!string.IsNullOrEmpty(_message))
                EditorGUILayout.HelpBox(_message, _messageType);

            EditorGUILayout.EndScrollView();
        }

        private void CreateAnimal()
        {
            AnimalDefinition animal = null;
            MovementDefinition movement = null;
            bool assetsSaved = false;

            try
            {
                ValidateInput();
                movement = Instantiate(_movementDraft);
                movement.hideFlags = HideFlags.None;
                movement.name = _speciesId + "Movement";
                animal = CreateInstance<AnimalDefinition>();
                animal.name = _speciesId;

                using (var serialized = new SerializedObject(animal))
                {
                    serialized.FindProperty("_speciesId").stringValue = _speciesId;
                    serialized.FindProperty("_foodRole").intValue = (int)_foodRole;
                    serialized.FindProperty("_prefab").objectReferenceValue = _prefab;
                    serialized.FindProperty("_movement").objectReferenceValue = movement;
                    serialized.ApplyModifiedPropertiesWithoutUndo();
                }

                animal.Validate();
                EnsureFolder(AnimalsFolder);
                EnsureFolder(MovementFolder);

                string movementPath = AssetDatabase.GenerateUniqueAssetPath($"{MovementFolder}/{_speciesId}Movement.asset");
                string animalPath = AssetDatabase.GenerateUniqueAssetPath($"{AnimalsFolder}/{_speciesId}.asset");
                AssetDatabase.CreateAsset(movement, movementPath);
                AssetDatabase.CreateAsset(animal, animalPath);
                AssetDatabase.SaveAssetIfDirty(movement);
                AssetDatabase.SaveAssetIfDirty(animal);
                assetsSaved = true;

                Selection.activeObject = animal;
                EditorGUIUtility.PingObject(animal);

                if (_addToSpawn)
                {
                    using (var serialized = new SerializedObject(_spawnSettings))
                    {
                        SerializedProperty animals = serialized.FindProperty("_animals");
                        int index = animals.arraySize;
                        animals.arraySize++;
                        animals.GetArrayElementAtIndex(index).objectReferenceValue = animal;
                        serialized.ApplyModifiedProperties();
                    }

                    AssetDatabase.SaveAssetIfDirty(_spawnSettings);
                }

                _message = $"Созданы {animalPath} и {movementPath}.";

                if (_addToSpawn)
                    _message += $" Животное добавлено в {_spawnSettings.name}.";

                _messageType = MessageType.Info;
            }
            catch (Exception exception)
            {
                _message = assetsSaved
                    ? $"Ассеты созданы. Проверь добавление в Spawn Settings: {exception.Message}"
                    : exception.Message;
                _messageType = MessageType.Error;
            }
            finally
            {
                if (!assetsSaved)
                {
                    RemoveCreatedObject(animal);
                    RemoveCreatedObject(movement);
                }
            }
        }

        private void ValidateInput()
        {
            if (_movementDraft == null)
                throw new InvalidOperationException("В проекте нет доступных типов MovementDefinition.");

            if (!Regex.IsMatch(_speciesId, "^[a-z][a-z0-9_]*$"))
                throw new InvalidOperationException("Укажи Species Id: начни с латинской буквы, используй строчные буквы, цифры и _.");

            if (_prefab == null || !PrefabUtility.IsPartOfPrefabAsset(_prefab) || _prefab.transform.parent != null)
                throw new InvalidOperationException("Выбери префаб из Project с AnimalBehaviour на корневом объекте.");

            if (!_prefab.enabled || !_prefab.gameObject.activeSelf)
                throw new InvalidOperationException("Префаб и его AnimalBehaviour должны быть включены.");

            foreach (string guid in AssetDatabase.FindAssets("t:AnimalDefinition"))
            {
                var existing = AssetDatabase.LoadAssetAtPath<AnimalDefinition>(AssetDatabase.GUIDToAssetPath(guid));

                if (existing != null && existing.SpeciesId == _speciesId)
                    throw new InvalidOperationException($"Species Id '{_speciesId}' уже используется в {AssetDatabase.GetAssetPath(existing)}.");
            }

            if (!_addToSpawn)
                return;

            if (_spawnSettings == null || !AssetDatabase.Contains(_spawnSettings))
                throw new InvalidOperationException("Назначь ассет Spawn Settings или выключи добавление в спавн.");

            if ((_spawnSettings.AnimalLayers & (1 << _prefab.gameObject.layer)) == 0)
                throw new InvalidOperationException("Слой префаба должен входить в Spawn Settings / Animal Layers.");
        }

        private void DrawMovement()
        {
            if (_movementTypes.Length == 0)
            {
                EditorGUILayout.HelpBox("Добавь конкретный класс, унаследованный от MovementDefinition.", MessageType.Info);
                return;
            }

            int index = EditorGUILayout.Popup("Movement", _movementIndex, _movementNames);

            if (index != _movementIndex)
            {
                _movementJson = null;
                CreateMovementDraft(index);
            }

            _movementProperties.Update();

            using (SerializedProperty property = _movementProperties.GetIterator())
            {
                bool enterChildren = true;

                while (property.NextVisible(enterChildren))
                {
                    enterChildren = false;

                    if (property.name != "m_Script")
                        EditorGUILayout.PropertyField(property, true);
                }
            }

            if (_movementProperties.ApplyModifiedPropertiesWithoutUndo())
                _movementJson = EditorJsonUtility.ToJson(_movementDraft);
        }

        private void CreateMovementDraft(int index)
        {
            ReleaseMovementDraft();
            _movementIndex = index;
            _movementTypeName = _movementTypes[index].AssemblyQualifiedName;
            _movementDraft = (MovementDefinition)CreateInstance(_movementTypes[index]);

            if (!string.IsNullOrEmpty(_movementJson))
                EditorJsonUtility.FromJsonOverwrite(_movementJson, _movementDraft);

            _movementDraft.hideFlags = HideFlags.HideAndDontSave;
            _movementProperties = new SerializedObject(_movementDraft);
        }

        private void ReleaseMovementDraft()
        {
            _movementProperties?.Dispose();
            _movementProperties = null;

            if (_movementDraft != null)
                DestroyImmediate(_movementDraft);

            _movementDraft = null;
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
                return;

            int separator = path.LastIndexOf('/');
            string parent = path.Substring(0, separator);
            EnsureFolder(parent);

            if (string.IsNullOrEmpty(AssetDatabase.CreateFolder(parent, path.Substring(separator + 1))))
                throw new InvalidOperationException($"Не удалось создать папку {path}.");
        }

        private static void RemoveCreatedObject(Object instance)
        {
            if (instance == null)
                return;

            string path = AssetDatabase.GetAssetPath(instance);

            if (string.IsNullOrEmpty(path))
                DestroyImmediate(instance);
            else
                AssetDatabase.DeleteAsset(path);
        }
    }
}
