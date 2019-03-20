using UnityEngine;

public class MoveCharacterController : MonoBehaviour
{
    public KeyCode
        GoRight = KeyCode.D,
        GoLeft = KeyCode.A,
        GoDown = KeyCode.S,
        Jump = KeyCode.Space,
        Run = KeyCode.LeftShift;

    private MovableCharacter player;
    private bool[] inputs;
    private bool[] prevInputs;

    void Start ()
    {
        player = GetComponent<MovableCharacter>();
        inputs = new bool[(int)KeyInput.Count];
        prevInputs = new bool[(int)KeyInput.Count];
        player.Init(inputs, prevInputs);
    }
    
    void Update()
    {
        inputs[(int)KeyInput.GoRight] = Input.GetKey(GoRight);
        inputs[(int)KeyInput.GoLeft] = Input.GetKey(GoLeft);
        inputs[(int)KeyInput.GoDown] = Input.GetKey(GoDown);
        inputs[(int)KeyInput.Jump] = Input.GetKey(Jump);
        inputs[(int)KeyInput.Run] = Input.GetKey(Run);
    }
}
