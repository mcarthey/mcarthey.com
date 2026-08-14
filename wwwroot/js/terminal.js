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

        switch (e.key) {
            case 'Enter':
                e.preventDefault();
                this.executeCommand(commandInput.value.trim().toUpperCase());
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
        // and any future hack-puzzle commands — discovery is part of the fun.
        const commands = ['HELP', 'CLEAR', 'SHAME', 'DIR', 'VER', 'TIME', 'DATE'];
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

        // Process the command
        switch (command.toUpperCase()) {
            case 'HELP':
                this.showHelp();
                break;
            case 'CLEAR':
                this.clearScreen();
                break;
            case 'KONAMI':
                this.triggerKonamiEasterEgg();
                break;
            case 'PACMAN':
                this.triggerPacmanEasterEgg();
                break;
            case 'TETRIS':
                this.triggerTetrisEasterEgg();
                break;
            case 'SHAME':
                window.location.href = '/Shame';
                break;
            case 'DIR':
                this.showDirectory();
                break;
            case 'VER':
                this.showVersion();
                break;
            case 'TIME':
                this.showTime();
                break;
            case 'DATE':
                this.showDate();
                break;
            default:
                this.displayOutput(`'${command}' is not recognized as an internal or external command, operable program or batch file.`, 'error');
                this.displayOutput(`Type HELP for available commands.`, 'info');
        }
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
        this.displayOutput('DIR      - List directory contents');
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

    showDirectory() {
        this.displayOutput('');
        this.displayOutput(' Directory of /', 'success');
        this.displayOutput('');
        this.displayOutput('drwxr-xr-x  root  4096  Aug 14 22:00  etc/');
        this.displayOutput('drwxr-xr-x  root  4096  Aug 14 22:00  var/');
        this.displayOutput('drwxr-xr-x  root  4096  Aug 14 22:00  home/');
        this.displayOutput('drwxr-xr-x  root  4096  Aug 14 22:00  usr/');
        this.displayOutput('-rw-r--r--  root   256  Aug 14 22:00  README');
        this.displayOutput('');
        this.displayOutput('Access to /etc, /var, /home restricted to authenticated users.');
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

    triggerKonamiEasterEgg() {
        this.displayOutput('');
        this.displayOutput('╔═══════════════════════════════════════╗', 'success');
        this.displayOutput('║     KONAMI CODE ACTIVATED!           ║', 'success');
        this.displayOutput('╚═══════════════════════════════════════╝', 'success');
        this.displayOutput('');
        this.displayOutput('UP UP DOWN DOWN LEFT RIGHT LEFT RIGHT B A');
        this.displayOutput('');
        this.displayOutput('You have unlocked: 30 Lives Mode!');
        this.displayOutput('Achievement: "The Code Master" earned!');

        // Add visual effects
        document.body.style.background = '#000080';
        setTimeout(() => {
            document.body.style.background = '#000000';
        }, 3000);
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
