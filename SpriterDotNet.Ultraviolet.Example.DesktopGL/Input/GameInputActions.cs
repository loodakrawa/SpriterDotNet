using TwistedLogik.Ultraviolet;
using TwistedLogik.Ultraviolet.Input;

namespace UltravioletGame1.Input
{
    public sealed class GameInputActions : InputActionCollection
    {
        public GameInputActions(UltravioletContext uv) : base(uv)
        {
        }

        public InputAction SwitchEntity { get; private set; }
        public InputAction NextAnimation { get; private set; }
        public InputAction IncreaseSpeed { get; private set; }
        public InputAction DecreaseSpeed { get; private set; }
        public InputAction ReverseAnimation { get; private set; }
        public InputAction Transition { get; private set; }
        public InputAction PushCharMap { get; private set; }
        public InputAction PopCharMap { get; private set; }
        public InputAction ToggleColour { get; private set; }
        public InputAction MoveUp { get; private set; }
        public InputAction MoveDown { get; private set; }
        public InputAction MoveLeft { get; private set; }
        public InputAction MoveRight { get; private set; }
        public InputAction RotateLeft { get; private set; }
        public InputAction RotateRight { get; private set; }
        public InputAction ScaleUp { get; private set; }
        public InputAction ScaleDown { get; private set; }
        public InputAction FlipX { get; private set; }
        public InputAction FlipY { get; private set; }
        public InputAction DrawOutlines { get; private set; }

        
        protected override void OnCreatingActions()
        {
            SwitchEntity = CreateAction("SwitchEntity");
            NextAnimation = CreateAction("NextAnimation");
            IncreaseSpeed = CreateAction("IncreaseSpeed");
            DecreaseSpeed = CreateAction("DecreaseSpeed");
            ReverseAnimation = CreateAction("ReverseAnimation");
            Transition = CreateAction("Transition");
            PushCharMap = CreateAction("PushCharMap");
            PopCharMap = CreateAction("PopCharMap");
            ToggleColour = CreateAction("ToggleColour");
            MoveUp = CreateAction("MoveUp");
            MoveDown = CreateAction("MoveDown");
            MoveLeft = CreateAction("MoveLeft");
            MoveRight = CreateAction("MoveRight");
            RotateLeft = CreateAction("RotateLeft");
            RotateRight = CreateAction("RotateRight");
            ScaleUp = CreateAction("ScaleUp");
            ScaleDown = CreateAction("ScaleDown");
            FlipX = CreateAction("FlipX");
            FlipY = CreateAction("FlipY");
            DrawOutlines = CreateAction("DrawOutlines");

            base.OnCreatingActions();
        }

        protected override void OnResetting()
        {
            SwitchEntity.Primary = CreateKeyboardBinding(Key.Return);
            NextAnimation.Primary = CreateKeyboardBinding(Key.Space);
            IncreaseSpeed.Primary = CreateKeyboardBinding(Key.P);
            DecreaseSpeed.Primary = CreateKeyboardBinding(Key.O);
            ReverseAnimation.Primary = CreateKeyboardBinding(Key.R);
            Transition.Primary = CreateKeyboardBinding(Key.T);
            PushCharMap.Primary = CreateKeyboardBinding(Key.C);
            PopCharMap.Primary = CreateKeyboardBinding(Key.V);
            ToggleColour.Primary = CreateKeyboardBinding(Key.J);
            MoveUp.Primary = CreateKeyboardBinding(Key.W);
            MoveDown.Primary = CreateKeyboardBinding(Key.S);
            MoveLeft.Primary = CreateKeyboardBinding(Key.A);
            MoveRight.Primary = CreateKeyboardBinding(Key.D);
            RotateLeft.Primary = CreateKeyboardBinding(Key.Q);
            RotateRight.Primary = CreateKeyboardBinding(Key.E);
            ScaleUp.Primary = CreateKeyboardBinding(Key.N);
            ScaleDown.Primary = CreateKeyboardBinding(Key.M);
            FlipX.Primary = CreateKeyboardBinding(Key.F);
            FlipY.Primary = CreateKeyboardBinding(Key.G);
            DrawOutlines.Primary = CreateKeyboardBinding(Key.BackQuote);

            base.OnResetting();
        }
    }
}
