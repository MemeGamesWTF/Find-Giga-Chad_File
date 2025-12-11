// OpenGameSDK.jslib
mergeInto(LibraryManager.library, {
  OGP_Init: function(gameIdPtr, playerIdPtr) {
    var gameId = UTF8ToString(gameIdPtr);
    var playerId = UTF8ToString(playerIdPtr);

    // If the SDK isn't loaded, log and try to continue (you may choose to inject script here).
    if (!window.OpenGameSDK) {
      console.error('OpenGameSDK not found on window. Make sure the script is included in index.html.');
      return;
    }

    try {
      var config = {
        ui: { usePointsWidget: true },
        logLevel: 1
      };
      var ogp = new window.OpenGameSDK(config);
      window._ogp = ogp;

      // initialize
      ogp.init({ gameId: gameId, playerId: playerId });

      // forward some events to Unity GameObject named "GameManager"
      ogp.on('OnReady', function() {
        // call GameManager.OnOGPReady (no payload)
        if (typeof SendMessage === 'function') SendMessage('GameManager', 'OnOGPReady', '');
      });

      ogp.on('OnSessionStarted', function() {
        if (typeof SendMessage === 'function') SendMessage('GameManager', 'OnOGPSessionStarted', '');
      });

      // optional: you can forward other events similarly
    } catch (e) {
      console.error('OGP Init error:', e);
    }
  },

  OGP_AddPoints: function(amount) {
    var pts = amount | 0;
    if (window._ogp && typeof window._ogp.addPoints === 'function') {
      window._ogp.addPoints(pts);
    } else {
      console.warn('OGP addPoints not available.');
    }
  },

  OGP_SavePoints: function(points) {
    var pts = points | 0;
    if (window._ogp && typeof window._ogp.savePoints === 'function') {
      window._ogp.savePoints(pts);
    } else {
      console.warn('OGP savePoints not available.');
    }
  },

  OGP_GameReadyToPlay: function() {
    if (window._ogp && typeof window._ogp.gameReadyToPlay === 'function') {
      window._ogp.gameReadyToPlay();
    } else {
      console.warn('OGP gameReadyToPlay not available.');
    }
  }
});
