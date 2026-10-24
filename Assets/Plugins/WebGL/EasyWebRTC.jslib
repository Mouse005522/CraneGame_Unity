mergeInto(LibraryManager.library, {
  $EasyRTC: {
    sessions: Object.create(null),
    close: function (id) {
      var s = EasyRTC.sessions[id];
      if (!s) return;
      delete EasyRTC.sessions[id];
      if (s.cancelIce) s.cancelIce();
      Object.keys(s.channels).forEach(function (key) { s.channels[key].close(); });
      s.pc.close();
    }
  },
  EasyRTC_Init__deps: ['$EasyRTC', '$SendMessage'],
  EasyRTC_Init: function (receiver, context, onData, onChannelState, onPeerState) {
    var id = UTF8ToString(receiver);
    var pc = new RTCPeerConnection({iceServers: []});
    var s = {
      pc: pc,
      channels: Object.create(null),
      offerStarted: false,
      context: context,
      onData: onData,
      onChannelState: onChannelState,
      onPeerState: onPeerState
    };
    EasyRTC.sessions[id] = s;
    pc.onconnectionstatechange = function () {
      if (EasyRTC.sessions[id] === s)
      {
        var states = {new: 0, connecting: 1, connected: 2, disconnected: 3, failed: 4, closed: 5};
        var state = states[pc.connectionState];
        if (state !== undefined)
        {
          {{{ makeDynCall('vii', 's.onPeerState') }}}(s.context, state);
        }
      }
    };
  },
  EasyRTC_BindChannel__deps: ['$EasyRTC', '$SendMessage', 'malloc', 'free'],
  EasyRTC_BindChannel: function (receiver, channelId, name, ordered, maxRetransmits) {
    var id = UTF8ToString(receiver), s = EasyRTC.sessions[id];
    if (!s || s.offerStarted) throw new Error('Bind channels before creating an offer');
    if (s.channels[channelId]) throw new Error('Duplicate channel id');
    var options = {ordered: !!ordered};
    // -1 表示不限重試，省略選項，不傳入負值。
    if (maxRetransmits !== -1) options.maxRetransmits = maxRetransmits;
    var dc = s.channels[channelId] = s.pc.createDataChannel(UTF8ToString(name), options);
    dc.binaryType = 'arraybuffer';
    function alive() {return EasyRTC.sessions[id] === s;}
    function state() {
      if (alive())
      {
        var states = {connecting: 0, open: 1, closing: 2, closed: 3};
        var value = states[dc.readyState];
        if (value !== undefined)
        {
          {{{ makeDynCall('viii', 's.onChannelState') }}}(s.context, channelId, value);
        }
      }
    }
    dc.onopen = state;
    dc.onclosing = state;
    dc.onclose = state;
    dc.onmessage = function (event) {
      if (!alive() || dc.readyState === 'closed' || !(event.data instanceof ArrayBuffer)) return;
      var bytes = new Uint8Array(event.data);
      var pointer = 0;
      try
      {
        if (bytes.length > 0)
        {
          pointer = _malloc(bytes.length);
          if (!pointer)
          {
            throw new Error('WebRTC receive buffer allocation failed');
          }
          // malloc 可能使 WASM memory 成長，必須在配置後取得最新的 HEAPU8。
          HEAPU8.set(bytes, pointer);
        }
        {{{ makeDynCall('viipi', 's.onData') }}}(s.context, channelId, pointer, bytes.length);
      }
      finally
      {
        // C# 的同步 Span 事件已執行完畢，可釋放 buffer；空封包不需要配置。
        if (pointer)
        {
          _free(pointer);
        }
      }
    };
    dc.onerror = function () {
      if (alive()) SendMessage(id, 'OnBrowserError', 'DataChannel error: ' + dc.label);
    };
  },
  EasyRTC_CreateOffer__deps: ['$EasyRTC', '$SendMessage'],
  EasyRTC_CreateOffer: function (receiver, timeout) {
    var id = UTF8ToString(receiver), s = EasyRTC.sessions[id];
    if (!s) return;
    var pc = s.pc;
    s.offerStarted = true;
    function alive() {return EasyRTC.sessions[id] === s;}
    (async function () {
      try {
        await pc.setLocalDescription(await pc.createOffer());
        if (!alive()) return;
        await new Promise(function (resolve, reject) {
          var timer;
          function finish(error) {
            clearTimeout(timer);
            pc.removeEventListener('icegatheringstatechange', check);
            s.cancelIce = null;
            if (error) reject(error); else resolve();
          }
          function check() {if (pc.iceGatheringState === 'complete') finish();}
          s.cancelIce = function () {finish(new Error('Connection closed'));};
          timer = setTimeout(function () {finish(new Error('ICE gathering timed out'));}, timeout * 1000);
          pc.addEventListener('icegatheringstatechange', check);
          check();
        });
        if (alive()) SendMessage(id, 'OnBrowserOffer', pc.localDescription.sdp);
      } catch (error) {
        if (alive()) {
          SendMessage(id, 'OnBrowserError', String(error.message || error));
          EasyRTC.close(id);
        }
      }
    })();
  },
  EasyRTC_SetAnswer__deps: ['$EasyRTC', '$SendMessage'],
  EasyRTC_SetAnswer: function (receiver, answer) {
    var id = UTF8ToString(receiver), sdp = UTF8ToString(answer), s = EasyRTC.sessions[id];
    if (!s) return;
    (async function () {
      try {
        await s.pc.setRemoteDescription({type: 'answer', sdp: sdp});
        if (EasyRTC.sessions[id] === s) SendMessage(id, 'OnBrowserAnswer', '');
      } catch (error) {
        if (EasyRTC.sessions[id] === s) {
          SendMessage(id, 'OnBrowserError', String(error.message || error));
          EasyRTC.close(id);
        }
      }
    })();
  },
  EasyRTC_Send__deps: ['$EasyRTC'],
  EasyRTC_Send: function (receiver, channelId, data, length) {
    var s = EasyRTC.sessions[UTF8ToString(receiver)], dc = s && s.channels[channelId];
    if (!dc || dc.readyState !== 'open' || s.pc.connectionState !== 'connected') return 0;
    try {
      // 同步複製，C# Span 會在 Send 返回後解除固定。
      dc.send(HEAPU8.slice(data, data + length));
      return 1;
    } catch (error) {return 0;}
  },
  EasyRTC_CloseChannel__deps: ['$EasyRTC'],
  EasyRTC_CloseChannel: function (receiver, channelId) {
    var s = EasyRTC.sessions[UTF8ToString(receiver)];
    if (s && s.channels[channelId]) s.channels[channelId].close();
  },
  EasyRTC_Close__deps: ['$EasyRTC'],
  EasyRTC_Close: function (receiver) {EasyRTC.close(UTF8ToString(receiver));}
});
