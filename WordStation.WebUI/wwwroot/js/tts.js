// Global TTS: herhangi bir yerden çağrılabilir
let _wdKeepAlive = null;
window.speakWord = function(word, btnElement = null) {
    if (!window.speechSynthesis || !word || !word.trim()) return;
    if (_wdKeepAlive) { clearInterval(_wdKeepAlive); _wdKeepAlive = null; }
    window.speechSynthesis.cancel();
    
    // Find targets to animate
    const targetBtns = btnElement ? [btnElement] : [];
    if (!btnElement) {
        const modalBtn = document.getElementById('detailSpeakBtn');
        if (modalBtn) targetBtns.push(modalBtn);
        const fcBtn = document.getElementById('speakBtn');
        if (fcBtn) targetBtns.push(fcBtn);
    }

    setTimeout(() => {
        const voices = window.speechSynthesis.getVoices();
        const usVoice =
            voices.find(v => v.lang === 'en-US' && v.localService) ||
            voices.find(v => v.lang === 'en-US') ||
            voices.find(v => v.lang.startsWith('en'));
        const u = new SpeechSynthesisUtterance(word.trim());
        u.rate = 0.85; u.pitch = 1; u.volume = 1; u.lang = 'en-US';
        if (usVoice) u.voice = usVoice;
        
        targetBtns.forEach(btn => {
            btn.style.color = '#63b3ed';
            const icon = btn.querySelector('i');
            if (icon) {
                icon.classList.remove('bi-volume-off-fill', 'bi-volume-off', 'bi-volume-up');
                icon.classList.add('bi-volume-up-fill');
            }
        });
        
        u.onstart = () => {
            _wdKeepAlive = setInterval(() => {
                if (!window.speechSynthesis.speaking) { clearInterval(_wdKeepAlive); return; }
                window.speechSynthesis.pause(); window.speechSynthesis.resume();
            }, 10000);
        };
        u.onend = u.onerror = () => {
            clearInterval(_wdKeepAlive);
            targetBtns.forEach(btn => {
                btn.style.color = 'rgba(255,255,255,0.75)';
                const icon = btn.querySelector('i');
                if (icon) {
                    icon.classList.remove('bi-volume-up-fill');
                    icon.classList.add('bi-volume-off-fill');
                }
            });
        };
        window.speechSynthesis.speak(u);
    }, 150);
};
