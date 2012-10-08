/*
 * Chromeless player has no controls.
 */
var ytplayer;

// Update a particular HTML element with a new value
function updateHTML(elmId, value) {
    document.getElementById(elmId).innerHTML = value;
}

// This function is called when an error is thrown by the player
function onPlayerError(errorCode) {
    alert("An error occured of type:" + errorCode);
}

// This function is called when the player changes state
function onPlayerStateChange(newState) {
    updateHTML("playerState", newState);
}

// Allow the user to set the volume from 0-100
function setVideoVolume() {
    var volume = parseInt(document.getElementById("volumeSetting").value);
    if(isNaN(volume) || volume < 0 || volume > 100) {
        alert("Please enter a valid volume between 0 and 100.");
    }
    else if(ytplayer){
        ytplayer.setVolume(volume);
    }
}

function playVideo() {
    if (ytplayer) {
        ytplayer.playVideo();
    }
}

function pauseVideo() {
    if (ytplayer) {
        ytplayer.pauseVideo();
    }
}

function muteVideo() {
    if(ytplayer) {
        ytplayer.mute();
    }
}

function unMuteVideo() {
    if(ytplayer) {
        ytplayer.unMute();
    }
}

function loadVideo(videoId) {
    if(ytplayer) {
        ytplayer.cueVideoById(videoId);
        ytplayer.playVideo();
    }
}


// This function is automatically called by the player once it loads
function onYouTubePlayerReady(playerId) {
    ytplayer = document.getElementById("ytPlayer");
    ytplayer.addEventListener("onStateChange", "onPlayerStateChange");
    ytplayer.addEventListener("onError", "onPlayerError");
}

// The "main method" of this sample. 
function loadPlayer() {
    // Lets Flash from another domain call JavaScript
    var params = { allowScriptAccess: "always" };
    // The element id of the Flash embed
    var atts = { id: "ytPlayer" };
    // All of the magic handled by SWFObject (http://code.google.com/p/swfobject/)
    swfobject.embedSWF("http://www.youtube.com/apiplayer?" + "version=3&enablejsapi=1&playerapiid=player1", "videoDiv", "500", "300", "9", null, null, params, atts);
}