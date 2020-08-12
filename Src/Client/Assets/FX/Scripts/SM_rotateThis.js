var rotationSpeedX:float=90;
var rotationSpeedY:float=0;
var rotationSpeedZ:float=0;
private var rotationVector:Vector3=Vector3(rotationSpeedX,rotationSpeedY,rotationSpeedZ);

private var startX : float;
private var startY : float;
private var startZ : float;
private var firstEnable : boolean = true;

function OnEnable(){
    if (firstEnable) {
        startX = transform.localEulerAngles.x;
        startY = transform.localEulerAngles.y;
        startZ = transform.localEulerAngles.z;
        firstEnable = false;
    }
    else
        transform.localEulerAngles = Vector3(startX, startY, startZ);
}

function Update () {
    transform.Rotate(Vector3(rotationSpeedX,rotationSpeedY,rotationSpeedZ)*Time.deltaTime);
}