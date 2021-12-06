
let tHeight;

document.getElementById('choice').onchange = function () {
    if (this.value != 't') {
        document.getElementById('t').style.visibility = "hidden";
        tHeight = document.getElementById('t').style.height;
        document.getElementById('t').style.height = "0px";
    }
    else {
        document.getElementById('t').style.visibility = "visible";
        document.getElementById('t').style.height = tHeight;
    }
}