// MCARTHEY-VAX-01 SYSTEM CONSOLE — client-side terminal shell.
class RetroTerminal {
    constructor() {
        this.commandHistory = [];
        this.historyIndex = -1;
        this.currentPath = 'C:\\>';
        this.isBooting = true;
        this.bootSkipped = false;
        this.konamiCode = [];
        this.konamiSequence = [38, 38, 40, 40, 37, 39, 37, 39, 66, 65]; // Up Up Down Down Left Right Left Right B A

        this.init();
    }

    init() {
        this.setupEventListeners();
        this.startBootSequence();
        this.setupKonamiCode();
    }

    setupEventListeners() {
        const commandInput = document.getElementById('command-input');
        if (commandInput) {
            commandInput.addEventListener('keydown', (e) => this.handleKeyDown(e));
        }

        // Auto-focus on command input when visible
        setTimeout(() => {
            if (commandInput && !this.isBooting) commandInput.focus();
        }, 1000);

        // Clicking anywhere in the terminal interface refocuses the input,
        // so users can never get "stuck" with a cursor that's scrolled off
        // the viewport. Skip if the click was on an interactive control.
        const terminal = document.getElementById('terminal-interface');
        if (terminal) {
            terminal.addEventListener('click', (e) => {
                const tag = e.target.tagName;
                if (tag === 'INPUT' || tag === 'BUTTON' || tag === 'A') return;
                document.getElementById('command-input')?.focus();
            });
        }
    }

    setupKonamiCode() {
        document.addEventListener('keydown', (e) => {
            this.konamiCode.push(e.keyCode);
            if (this.konamiCode.length > this.konamiSequence.length) {
                this.konamiCode.shift();
            }

            if (this.konamiCode.length === this.konamiSequence.length &&
                this.konamiCode.every((code, index) => code === this.konamiSequence[index])) {
                this.triggerKonamiEasterEgg();
            }
        });
    }

    startBootSequence() {
        // Check if user wants to skip boot (stored in localStorage)
        const skipBoot = localStorage.getItem('skipBootSequence') === 'true';

        if (skipBoot) {
            this.skipBootSequence();
            return;
        }

        const bootText = document.getElementById('boot-text');
        const bootMessages = [
            "MCARTHEY-VAX-01 SYSTEM CONSOLE",
            "VMS V7.3-2 (bootstrap loaded)",
            "",
            "Memory check ....... 128MB OK",
            "Disk DUA0: ......... [SYSDEVICE] mounted",
            "Disk DUA1: ......... [USERDATA]  mounted",
            "Network XQA0: ...... online (10.0.0.7)",
            "Audit subsystem .... /var/log/access ACTIVE",
            "IDS subsystem ...... /var/log/intrusion ACTIVE",
            "",
            "WARNING: This system is monitored. All access is logged.",
            "",
            "SYSTEM READY.",
            ""
        ];

        let messageIndex = 0;
        let charIndex = 0;
        let currentMessage = '';

        const typeBootMessage = () => {
            if (this.bootSkipped) return;

            if (messageIndex < bootMessages.length) {
                if (charIndex < bootMessages[messageIndex].length) {
                    currentMessage += bootMessages[messageIndex][charIndex];
                    bootText.innerHTML = currentMessage.replace(/\n/g, '<br>');
                    charIndex++;
                    // Faster typing - reduced from 50ms to 15ms
                    setTimeout(typeBootMessage, 15);
                } else {
                    messageIndex++;
                    charIndex = 0;
                    currentMessage += '\n';
                    // Faster line delay - reduced from 200ms to 100ms
                    setTimeout(typeBootMessage, 100);
                }
            } else {
                this.showWelcomeScreen();
            }
        };

        typeBootMessage();
    }

    skipBootSequence() {
        this.bootSkipped = true;
        this.showWelcomeScreen();
    }

    showWelcomeScreen() {
        const bootSequence = document.getElementById('boot-sequence');
        const skipButton = document.getElementById('skip-button');

        if (bootSequence) bootSequence.style.display = 'none';
        if (skipButton) skipButton.style.display = 'none';

        document.getElementById('welcome-header')?.classList.remove('hidden');
        document.getElementById('welcome-header')?.classList.add('fade-in');

        setTimeout(() => {
            document.getElementById('nav-section')?.classList.remove('hidden');
            document.getElementById('nav-section')?.classList.add('fade-in');
        }, 500); // Faster - reduced from 1000ms

        setTimeout(() => {
            document.getElementById('command-interface')?.classList.remove('hidden');
            document.getElementById('command-interface')?.classList.add('fade-in');
        }, 800); // Faster - reduced from 1500ms

        setTimeout(() => {
            document.getElementById('system-info')?.classList.remove('hidden');
            document.getElementById('system-info')?.classList.add('fade-in');
            this.isBooting = false;

            // Auto-focus command input
            const commandInput = document.getElementById('command-input');
            if (commandInput) commandInput.focus();
        }, 1100); // Faster - reduced from 2000ms
    }

    handleKeyDown(e) {
        if (this.isBooting) return;

        const commandInput = document.getElementById('command-input');

        // LOGIN state: capture raw input (no upper-case, no history),
        // handle username → password → submit flow.
        if (this.loginState) {
            if (e.key === 'Escape') {
                e.preventDefault();
                this.loginState = null;
                commandInput.value = '';
                commandInput.type = 'text';
                commandInput.placeholder = 'Type command or click navigation...';
                this.displayOutput('(login aborted)');
                return;
            }
            if (e.key !== 'Enter') return;
            e.preventDefault();
            const value = commandInput.value;
            commandInput.value = '';
            if (this.loginState.step === 'username') {
                this.loginState.username = value;
                this.displayOutput(`Username: ${value}`);
                commandInput.type = 'password';
                commandInput.placeholder = 'Password:';
                this.loginState.step = 'password';
                return;
            }
            // Password step
            const username = this.loginState.username;
            const password = value;
            this.displayOutput('Password: ' + '*'.repeat(Math.min(password.length, 8)));
            this.loginState = null;
            commandInput.type = 'text';
            commandInput.placeholder = 'Type command or click navigation...';
            this.completeLogin(username, password);
            return;
        }

        switch (e.key) {
            case 'Enter':
                e.preventDefault();
                // Preserve raw command for server-side dispatch; executeCommand
                // does its own upper-casing for the local-command match.
                this.executeCommand(commandInput.value.trim());
                break;
            case 'ArrowUp':
                e.preventDefault();
                this.navigateHistory(-1);
                break;
            case 'ArrowDown':
                e.preventDefault();
                this.navigateHistory(1);
                break;
            case 'Tab':
                e.preventDefault();
                this.autoComplete(commandInput.value);
                break;
        }
    }

    navigateHistory(direction) {
        if (this.commandHistory.length === 0) return;

        this.historyIndex += direction;

        if (this.historyIndex < -1) this.historyIndex = -1;
        if (this.historyIndex >= this.commandHistory.length) {
            this.historyIndex = this.commandHistory.length - 1;
        }

        const commandInput = document.getElementById('command-input');
        if (this.historyIndex === -1) {
            commandInput.value = '';
        } else {
            commandInput.value = this.commandHistory[this.historyIndex];
        }
    }

    autoComplete(partialCommand) {
        // Autocomplete deliberately excludes easter-egg commands (KONAMI, PACMAN, TETRIS)
        // and hack-puzzle discovery commands — surprise is part of the fun.
        const commands = ['HELP', 'CLEAR', 'SHAME', 'LOGIN', 'LS', 'CAT', 'WHOAMI', 'VER', 'TIME', 'DATE'];
        const matches = commands.filter(cmd => cmd.startsWith(partialCommand.toUpperCase()));

        if (matches.length === 1) {
            document.getElementById('command-input').value = matches[0];
        } else if (matches.length > 1) {
            this.displayOutput(`Available commands: ${matches.join(', ')}`);
        }
    }

    executeCommand(command) {
        if (!command) return;

        const commandInput = document.getElementById('command-input');
        this.commandHistory.push(command);
        this.historyIndex = -1;

        // Clear the input
        commandInput.value = '';

        // Display the command
        this.displayOutput(`C:\\> ${command}`, 'command');

        // Client-side commands: pure UX + easter eggs. Everything else
        // (LS, CAT, WHOAMI, HISTORY, WARGAMES, LOGOUT, etc.) goes to the
        // server so puzzle content never lives in view-source.
        const upper = command.toUpperCase();
        switch (upper) {
            case 'HELP':
                this.showHelp();
                return;
            case 'CLEAR':
                this.clearScreen();
                return;
            case 'KONAMI':
                this.triggerKonamiEasterEgg();
                return;
            case 'PACMAN':
                this.triggerPacmanEasterEgg();
                return;
            case 'TETRIS':
                this.triggerTetrisEasterEgg();
                return;
            case 'SHAME':
                window.location.href = '/Shame';
                return;
            case 'VER':
                this.showVersion();
                return;
            case 'TIME':
                this.showTime();
                return;
            case 'DATE':
                this.showDate();
                return;
            case 'LOGIN':
                this.beginLogin();
                return;
        }
        // Fall-through: send to server. Server decides whether the command
        // is a real fake-shell verb or "command not found".
        this.execRemote(command);
    }

    async execRemote(command) {
        try {
            const resp = await fetch('/api/hack/exec', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ command })
            });
            if (!resp.ok) {
                this.displayOutput(`(server error: HTTP ${resp.status})`, 'error');
                return;
            }
            const data = await resp.json();
            for (const line of (data.output || [])) {
                this.displayOutput(line);
            }
        } catch (err) {
            this.displayOutput(`(network error: ${err.message})`, 'error');
        } finally {
            this.refocusInput();
        }
    }

    beginLogin() {
        // Two-step interaction handled inside handleKeyDown via this.loginState.
        // On Enter with step='username', capture username, mask input, step='password'.
        // On Enter with step='password', POST creds, reset state.
        const input = document.getElementById('command-input');
        if (!input) return;
        input.value = '';
        input.placeholder = 'Username:';
        this.loginState = { step: 'username', username: null };
        this.displayOutput('Enter credentials. ESC to abort.');
    }

    async completeLogin(username, password) {
        try {
            const resp = await fetch('/api/hack/login', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ username, password })
            });
            const data = await resp.json();
            for (const line of (data.output || [])) {
                this.displayOutput(line);
            }
        } catch (err) {
            this.displayOutput(`(network error: ${err.message})`, 'error');
        } finally {
            this.refocusInput();
        }
    }

    // Refocus + scroll into view. Input sits above the command-output div,
    // so a long response can push the input above the viewport fold — the
    // cursor is there but the user can't see it. Called after any async
    // handler that renders output.
    refocusInput() {
        const input = document.getElementById('command-input');
        if (!input) return;
        input.focus();
        input.scrollIntoView({ behavior: 'smooth', block: 'center' });
    }

    displayOutput(message, type = 'normal') {
        const outputDiv = document.getElementById('command-output');
        if (!outputDiv) return;

        let className = 'terminal-text';
        if (type === 'error') className += ' error-text';
        if (type === 'success') className += ' success-text';

        const messageDiv = document.createElement('div');
        messageDiv.className = className;
        messageDiv.innerHTML = message;

        outputDiv.appendChild(messageDiv);
        outputDiv.scrollTop = outputDiv.scrollHeight;

        // Keep only last 20 messages
        while (outputDiv.children.length > 20) {
            outputDiv.removeChild(outputDiv.firstChild);
        }
    }

    showHelp() {
        this.displayOutput('');
        this.displayOutput('=== AVAILABLE COMMANDS ===', 'success');
        this.displayOutput('HELP     - Show this message');
        this.displayOutput('CLEAR    - Clear the screen');
        this.displayOutput('SHAME    - Wall of shame (recent intrusion attempts)');
        this.displayOutput('LS       - List directory contents (try LS /home)');
        this.displayOutput('CAT      - Print file contents (try CAT /etc/motd)');
        this.displayOutput('WHOAMI   - Show current user');
        this.displayOutput('LOGIN    - Authenticate to the system');
        this.displayOutput('LOGOUT   - End authenticated session');
        this.displayOutput('HISTORY  - Show recent shell commands');
        this.displayOutput('VER      - Show system version');
        this.displayOutput('TIME     - Display current time');
        this.displayOutput('DATE     - Display current date');
        this.displayOutput('');
        this.displayOutput('Arrow keys navigate command history. TAB auto-completes.');
    }

    clearScreen() {
        const outputDiv = document.getElementById('command-output');
        if (outputDiv) outputDiv.innerHTML = '';
    }

    showVersion() {
        this.displayOutput('');
        this.displayOutput('MCARTHEY-VAX-01 SYSTEM CONSOLE', 'success');
        this.displayOutput('Version 3.14.15  (build 8628)');
        this.displayOutput('Kernel:   VMS V7.3-2');
        this.displayOutput('Uptime:   14847 days, 02:41:17');
        this.displayOutput('Operator: [redacted]');
    }

    showTime() {
        const now = new Date();
        this.displayOutput(`Current time: ${now.toLocaleTimeString()}`);
    }

    showDate() {
        const now = new Date();
        this.displayOutput(`Current date: ${now.toLocaleDateString()}`);
    }

    async triggerKonamiEasterEgg() {
        // Guard against re-entry -- the konami keydown listener can fire
        // rapidly if the sequence completes twice.
        if (this._konamiRunning) return;
        this._konamiRunning = true;
        try {
            await this._runKonamiSequence();
        } finally {
            this._konamiRunning = false;
        }
    }

    async _runKonamiSequence() {
        // Phase 1: Screen glitch. Body-level CSS class does the shake +
        // color-inversion flicker. Runs while we build the overlay.
        document.body.classList.add('konami-glitch');
        await this._sleep(1200);
        document.body.classList.remove('konami-glitch');

        // Phase 2: Build full-screen overlay
        const overlay = this._buildKonamiOverlay();
        document.body.appendChild(overlay);
        const line = overlay.querySelector('.k-line');
        const write = async (txt, delay = 40) => {
            for (const ch of txt) {
                line.textContent += ch;
                await this._sleep(delay);
            }
            line.textContent += '\n';
        };

        // Phase 3: Dial sequence with DTMF tones
        await write('> Initializing 300 baud modem...', 25);
        await this._sleep(300);
        await write('> DIALING  5-5-5-0-1-8-7', 60);
        await this._playDialTones();
        await this._sleep(400);

        // Phase 4: Handshake (the iconic screech)
        await write('> RINGING...', 40);
        await this._sleep(600);
        await write('> CARRIER DETECTED. NEGOTIATING PROTOCOL...', 25);
        this._playModemHandshake();
        await this._sleep(3200);
        await write('> CONNECT 2400/ARQ/V32/LAP-M', 25);
        await this._sleep(400);

        // Phase 5: POST to server for the log entry + get client IP back
        let clientIp = 'unknown';
        try {
            const r = await fetch('/api/hack/konami', { method: 'POST' });
            const data = await r.json();
            clientIp = data.ip || 'unknown';
        } catch (e) { /* still show the animation */ }

        // Phase 6: The "you have been traced" moment
        await write('> AUTHENTICATION BYPASSED', 30);
        await this._sleep(300);
        await write('> ACCESS GRANTED  --  WOPR BACKDOOR', 30);
        await this._sleep(600);
        overlay.classList.add('k-red');
        await write('', 0);
        await write('* * * WARNING * * *', 45);
        await write('THIS SESSION IS BEING TRACED.', 30);
        await write(`ORIGIN IP: ${clientIp}`, 40);
        await write('IDENTIFICATION LOGGED TO CENTRAL DATABASE.', 25);
        await this._sleep(1200);
        await write('', 0);
        overlay.classList.remove('k-red');
        await write('  ...just kidding. this is a honeypot.', 35);
        await write('  ...you have been added to /Shame as a', 25);
        await write('  Konami-code enterer. wear it proudly.', 25);
        await this._sleep(2000);

        // Phase 7: Fade out overlay
        overlay.classList.add('k-fade');
        await this._sleep(600);
        overlay.remove();

        // Phase 8: Terminal follow-up
        this.displayOutput('');
        this.displayOutput('> KONAMI SEQUENCE ACCEPTED. see /Shame for your entry.', 'success');
        this.refocusInput();
    }

    _sleep(ms) { return new Promise(r => setTimeout(r, ms)); }

    _buildKonamiOverlay() {
        const div = document.createElement('div');
        div.className = 'konami-overlay';
        div.innerHTML = `
            <div class="k-inner">
              <pre class="k-phone">
     ______________
    |  ___________ |
    | |           ||     TRACE INITIATED
    | |    -_-    ||     ---------------
    | |___________||     ATT/BELL 2400 MODEM
    |_______________|    CONNECT VIA POTS
    |___._______.___|
       |||     |||
       |||=====|||
       |||     |||
      _|||     |||_
              </pre>
              <pre class="k-line"></pre>
            </div>
        `;
        return div;
    }

    // Web Audio: DTMF-ish tones for the dial pulses. Not real DTMF but
    // recognizable as touch-tone dialing to anyone who lived through it.
    async _playDialTones() {
        try {
            const ctx = this._audioCtx ||= new (window.AudioContext || window.webkitAudioContext)();
            const tones = [
                [770, 1336], [770, 1336], [770, 1336],  // 5 5 5
                [697, 1209], [852, 1209], [697, 1477], [770, 1477],  // 0 1 8 7 (approx)
            ];
            for (const [f1, f2] of tones) {
                this._playTonePair(ctx, f1, f2, 0.15);
                await this._sleep(180);
            }
        } catch (e) { /* audio can fail silently on locked contexts */ }
    }

    _playTonePair(ctx, f1, f2, dur) {
        const now = ctx.currentTime;
        for (const f of [f1, f2]) {
            const osc = ctx.createOscillator();
            const gain = ctx.createGain();
            osc.type = 'sine';
            osc.frequency.value = f;
            gain.gain.setValueAtTime(0.08, now);
            gain.gain.exponentialRampToValueAtTime(0.0001, now + dur);
            osc.connect(gain).connect(ctx.destination);
            osc.start(now);
            osc.stop(now + dur);
        }
    }

    // The iconic modem handshake screech. Layered oscillators + noise burst.
    _playModemHandshake() {
        try {
            const ctx = this._audioCtx ||= new (window.AudioContext || window.webkitAudioContext)();
            const now = ctx.currentTime;
            const dur = 3.0;

            // Carrier tone that warbles
            const carrier = ctx.createOscillator();
            const carrierGain = ctx.createGain();
            carrier.type = 'sawtooth';
            carrier.frequency.setValueAtTime(1080, now);
            carrier.frequency.linearRampToValueAtTime(2100, now + 0.6);
            carrier.frequency.linearRampToValueAtTime(1650, now + 1.4);
            carrier.frequency.linearRampToValueAtTime(2225, now + 2.2);
            carrierGain.gain.setValueAtTime(0.06, now);
            carrierGain.gain.linearRampToValueAtTime(0.001, now + dur);
            carrier.connect(carrierGain).connect(ctx.destination);
            carrier.start(now);
            carrier.stop(now + dur);

            // Warbling second tone
            const warble = ctx.createOscillator();
            const warbleGain = ctx.createGain();
            const lfo = ctx.createOscillator();
            const lfoGain = ctx.createGain();
            warble.type = 'square';
            warble.frequency.value = 1300;
            lfo.frequency.value = 7;
            lfoGain.gain.value = 200;
            lfo.connect(lfoGain).connect(warble.frequency);
            warbleGain.gain.setValueAtTime(0.05, now);
            warbleGain.gain.linearRampToValueAtTime(0.001, now + dur);
            warble.connect(warbleGain).connect(ctx.destination);
            warble.start(now);
            lfo.start(now);
            warble.stop(now + dur);
            lfo.stop(now + dur);

            // Noise burst -- the "krsshhhhh"
            const bufferSize = ctx.sampleRate * dur;
            const noiseBuffer = ctx.createBuffer(1, bufferSize, ctx.sampleRate);
            const data = noiseBuffer.getChannelData(0);
            for (let i = 0; i < bufferSize; i++) data[i] = (Math.random() * 2 - 1) * 0.5;
            const noise = ctx.createBufferSource();
            noise.buffer = noiseBuffer;
            const noiseGain = ctx.createGain();
            noiseGain.gain.setValueAtTime(0.04, now);
            noiseGain.gain.linearRampToValueAtTime(0.001, now + dur);
            noise.connect(noiseGain).connect(ctx.destination);
            noise.start(now);
            noise.stop(now + dur);
        } catch (e) { /* silent fallback */ }
    }

    triggerPacmanEasterEgg() {
        this.displayOutput('');
        this.displayOutput('┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┓', 'success');
        this.displayOutput('┃     PAC-MAN EASTER EGG!              ┃', 'success');
        this.displayOutput('┗━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┛', 'success');
        this.displayOutput('');
        this.displayOutput('WAKA WAKA WAKA WAKA WAKA WAKA WAKA WAKA');
        this.displayOutput('');
        this.displayOutput('Achievement: "Dot Eater" earned!');
    }

    triggerTetrisEasterEgg() {
        this.displayOutput('');
        this.displayOutput('▛▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▜', 'success');
        this.displayOutput('▌     TETRIS EASTER EGG!                ▐', 'success');
        this.displayOutput('▙▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▟', 'success');
        this.displayOutput('');
        this.displayOutput('Lines cleared: 999999');
        this.displayOutput('Level: MAX');
        this.displayOutput('');
        this.displayOutput('Achievement: "Line Master" earned!');
    }
}

// Global function for skip button
function skipBootSequence() {
    if (window.terminal) {
        // Remember preference
        localStorage.setItem('skipBootSequence', 'true');
        window.terminal.skipBootSequence();
    }
}

// Initialize terminal when DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
    // Only initialize on home page
    if (document.getElementById('terminal-interface')) {
        window.terminal = new RetroTerminal();
    }
});

// Add retro sound effects (optional)
function playBeep(frequency = 800, duration = 100) {
    try {
        const audioContext = new (window.AudioContext || window.webkitAudioContext)();
        const oscillator = audioContext.createOscillator();
        const gainNode = audioContext.createGain();

        oscillator.connect(gainNode);
        gainNode.connect(audioContext.destination);

        oscillator.frequency.value = frequency;
        oscillator.type = 'square';

        gainNode.gain.setValueAtTime(0.1, audioContext.currentTime);
        gainNode.gain.exponentialRampToValueAtTime(0.01, audioContext.currentTime + duration / 1000);

        oscillator.start(audioContext.currentTime);
        oscillator.stop(audioContext.currentTime + duration / 1000);
    } catch (e) {
        // Silent fail if Web Audio API not supported
    }
}

// Add click sound to navigation
document.addEventListener('click', (e) => {
    if (e.target.classList.contains('nav-icon') || e.target.closest('.nav-icon')) {
        playBeep(600, 50);
    }
});

// Add keyboard sound effects
document.addEventListener('keydown', (e) => {
    if (!window.terminal || window.terminal.isBooting) return;

    if (e.key.length === 1 || e.key === 'Enter' || e.key === 'Backspace') {
        playBeep(400, 20);
    }
});

// Console easter egg for developers
console.log(`
╔═══════════════════════════════════════╗
║     GEN-X DEVELOPER TERMINAL         ║
║     Console Easter Egg Discovered!    ║
╚═══════════════════════════════════════╝

Available console commands:
- terminal.executeCommand('command')
- playBeep(frequency, duration)
- skipBootSequence()

Fun facts:
- This terminal simulates a Commodore VIC-20
- Built with ASP.NET Core 8 and C# MVC
- Designed for SmarterASP.NET deployment
- Contains multiple easter eggs

Try typing KONAMI in the terminal!
`);
