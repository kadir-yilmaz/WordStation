// Global TTS: herhangi bir yerden çağrılabilir
let _wdKeepAlive = null;
window.speakWord = function(word) {
    if (!window.speechSynthesis || !word || !word.trim()) return;
    if (_wdKeepAlive) { clearInterval(_wdKeepAlive); _wdKeepAlive = null; }
    window.speechSynthesis.cancel();
    setTimeout(() => {
        const voices = window.speechSynthesis.getVoices();
        const usVoice =
            voices.find(v => v.lang === 'en-US' && v.localService) ||
            voices.find(v => v.lang === 'en-US') ||
            voices.find(v => v.lang.startsWith('en'));
        const u = new SpeechSynthesisUtterance(word.trim());
        u.rate = 0.85; u.pitch = 1; u.volume = 1; u.lang = 'en-US';
        if (usVoice) u.voice = usVoice;
        const btn = document.getElementById('detailSpeakBtn');
        if (btn) btn.style.color = '#63b3ed';
        u.onstart = () => {
            _wdKeepAlive = setInterval(() => {
                if (!window.speechSynthesis.speaking) { clearInterval(_wdKeepAlive); return; }
                window.speechSynthesis.pause(); window.speechSynthesis.resume();
            }, 10000);
        };
        u.onend = u.onerror = () => {
            clearInterval(_wdKeepAlive);
            const b = document.getElementById('detailSpeakBtn');
            if (b) b.style.color = 'rgba(255,255,255,0.75)';
        };
        window.speechSynthesis.speak(u);
    }, 150);
};
