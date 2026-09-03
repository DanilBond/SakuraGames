using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using ZooWorld.Animals.Definitions;
using ZooWorld.Animals.Movement;
using ZooWorld.World;

namespace ZooWorld.Physics.Tests
{
    public sealed class JumpMovementTests
    {
        private const float Step = 0.02f;
        private Scene _scene;
        private PhysicsScene _physics;
        private Rigidbody _body;
        private JumpMovementDefinition _definition;
        private JumpMovement _movement;
        private PhysicsMaterial _material;
        private Random.State _randomState;

        [SetUp]
        public void SetUp()
        {
            _randomState = Random.state;
            _scene = EditorSceneManager.NewPreviewScene();
            _physics = _scene.GetPhysicsScene();
            Assert.That(_physics, Is.Not.EqualTo(UnityEngine.Physics.defaultPhysicsScene),
                "Tests must not simulate the open gameplay scene.");

            var ground = CreateObject("Ground");
            ground.transform.position = new Vector3(0f, -0.5f, 0f);
            var groundCollider = ground.AddComponent<BoxCollider>();
            groundCollider.size = new Vector3(200f, 1f, 200f);
            _material = new PhysicsMaterial { bounciness = 0f, staticFriction = 0f, dynamicFriction = 0f };
            groundCollider.sharedMaterial = _material;

            var cameraObject = CreateObject("Camera");
            cameraObject.transform.SetPositionAndRotation(new Vector3(0f, 100f, 0f), Quaternion.Euler(90f, 0f, 0f));
            var camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 80f;
            camera.aspect = 1f;
            camera.enabled = false;
            var bounds = new WorldBoundsProvider(camera, 0f, 0.5f);
            bounds.Initialize();

            var animal = CreateObject("Frog");
            animal.transform.position = new Vector3(0f, 0.5f, 0f);
            var collider = animal.AddComponent<SphereCollider>();
            collider.radius = 0.5f;
            collider.sharedMaterial = _material;
            _body = animal.AddComponent<Rigidbody>();
            _body.useGravity = true;
            _body.linearDamping = 0f;
            _body.constraints = RigidbodyConstraints.FreezeRotation;
            _definition = ScriptableObject.CreateInstance<JumpMovementDefinition>();
            _definition.Validate();
            _movement = new JumpMovement(_body, bounds, _definition, 0.5f);
            _movement.Reset(Vector3.forward);
            UnityEngine.Physics.SyncTransforms();
        }

        [TearDown]
        public void TearDown()
        {
            if (_scene.IsValid())
                EditorSceneManager.ClosePreviewScene(_scene);

            Object.DestroyImmediate(_definition);
            Object.DestroyImmediate(_material);
            Random.state = _randomState;
        }

        [Test]
        public void JumpFollowsConfiguredArcAndWaitsAfterLanding()
        {
            for (int i = 0; i < 90; i++)
            {
                Tick();
                Assert.That(_body.position.y, Is.LessThan(0.55f));
            }

            WaitForTakeoff(30);
            float peak = _body.position.y;
            bool landed = false;

            for (int i = 0; i < 100; i++)
            {
                Tick();
                peak = Mathf.Max(peak, _body.position.y);

                if (_body.position.y < 0.54f && _body.linearVelocity.y <= 0.1f)
                {
                    landed = true;
                    break;
                }
            }

            Assert.That(landed, Is.True, "The frog should land after its first jump.");
            Assert.That(peak - 0.5f, Is.EqualTo(_definition.Height).Within(0.06f));
            Vector3 landingPosition = _body.position;
            landingPosition.y = 0f;
            Assert.That(landingPosition.magnitude, Is.EqualTo(_definition.Distance).Within(0.15f));

            for (int i = 0; i < 90; i++)
            {
                Tick();
                Assert.That(_body.position.y, Is.LessThan(0.55f), "The pause starts after landing.");
            }

            WaitForTakeoff(30);
        }

        [Test]
        public void ResetDuringFlightWaitsForLandingBeforeStartingANewJump()
        {
            WaitForTakeoff(130);
            _movement.OnObstacleContact(Vector3.back);
            _movement.OnCollision();
            _movement.Reset(Vector3.forward);
            _body.position = new Vector3(0f, 40f, 0f);
            _body.linearVelocity = Vector3.zero;
            _body.angularVelocity = Vector3.zero;

            bool landed = false;

            for (int i = 0; i < 250; i++)
            {
                Tick();

                if (_body.position.y <= 0.54f)
                {
                    landed = true;
                    break;
                }

                Assert.That(_body.linearVelocity.y, Is.LessThanOrEqualTo(0f), "No jump while falling.");
            }

            Assert.That(landed, Is.True);

            for (int i = 0; i < 90; i++)
            {
                Tick();
                Assert.That(_body.position.y, Is.LessThan(0.55f));
            }

            WaitForTakeoff(30);
        }

        private GameObject CreateObject(string name)
        {
            var instance = new GameObject(name);
            SceneManager.MoveGameObjectToScene(instance, _scene);
            return instance;
        }

        private void WaitForTakeoff(int maxSteps)
        {
            for (int i = 0; i < maxSteps; i++)
            {
                Tick();

                if (_body.linearVelocity.y > 0.5f)
                    return;
            }

            Assert.Fail("The frog did not jump after the ground pause.");
        }

        private void Tick()
        {
            _movement.FixedTick(Step);
            _physics.Simulate(Step);
        }
    }
}
