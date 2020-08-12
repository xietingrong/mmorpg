var translationSpeedX:float=0;
var translationSpeedY:float=1;
var translationSpeedZ:float=0;

var local:boolean=true;

private var startX : float;
private var startY : float;
private var startZ : float;
private var firstEnable : boolean = true;

function OnEnable(){
    if (firstEnable) {
        startX = transform.localPosition.x;
        startY = transform.localPosition.y;
        startZ = transform.localPosition.z;
        firstEnable = false;
    }
    else
        transform.localPosition = Vector3(startX, startY, startZ);
}



function Update () {
    if (local==true)
        transform.Translate(Vector3(translationSpeedX,translationSpeedY,translationSpeedZ)*Time.deltaTime);
    else
        transform.Translate(Vector3(translationSpeedX,translationSpeedY,translationSpeedZ)*Time.deltaTime, Space.World);
}