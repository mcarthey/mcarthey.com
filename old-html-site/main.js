// GEN-X DEVELOPER TERMINAL - Main JavaScript
class RetroTerminal {
    constructor() {
        this.commandHistory = [];
        this.historyIndex = -1;
        this.currentPath = 'C:\\>';
        this.isBooting = true;
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
            commandInput.addEventListener('input', (e) => this.handleInput(e));
        }
        
        // Auto-focus on command input
        setTimeout(() => {
            if (commandInput) commandInput.focus();
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
        const bootText = document.getElementById('boot-text');
        const bootMessages = [
            "COMMODORE VIC-20 EMULATION LOADING...",
            "MEMORY CHECK: 3583 BYTES FREE",
            "**** COMMODORE 64 BASIC V2 ****",
            "64K RAM SYSTEM  38911 BASIC BYTES FREE",
            "READY.",
            "",
            "GEN-X DEVELOPER TERMINAL v2025.12.10",
            "Initializing retro computing environment...",
            "Loading C# MVC framework...",
            "Starting command interface...",
            ""
        ];
        
        let messageIndex = 0;
        let charIndex = 0;
        let currentMessage = '';
        
        const typeBootMessage = () => {
            if (messageIndex < bootMessages.length) {
                if (charIndex < bootMessages[messageIndex].length) {
                    currentMessage += bootMessages[messageIndex][charIndex];
                    bootText.innerHTML = currentMessage.replace(/\n/g, '<br>');
                    charIndex++;
                    setTimeout(typeBootMessage, 50);
                } else {
                    messageIndex++;
                    charIndex = 0;
                    currentMessage += '\n';
                    setTimeout(typeBootMessage, 200);
                }
            } else {
                this.showWelcomeScreen();
            }
        };
        
        typeBootMessage();
    }
    
    showWelcomeScreen() {
        document.getElementById('boot-sequence').style.display = 'none';
        document.getElementById('welcome-header').classList.remove('hidden');
        document.getElementById('welcome-header').classList.add('fade-in');
        
        setTimeout(() => {
            document.getElementById('nav-section').classList.remove('hidden');
            document.getElementById('nav-section').classList.add('fade-in');
        }, 1000);
        
        setTimeout(() => {
            document.getElementById('command-interface').classList.remove('hidden');
            document.getElementById('command-interface').classList.add('fade-in');
        }, 1500);
        
        setTimeout(() => {
            document.getElementById('system-info').classList.remove('hidden');
            document.getElementById('system-info').classList.add('fade-in');
            this.isBooting = false;
        }, 2000);
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
    
    handleInput(e) {
        // Auto-complete suggestions could be implemented here
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
        const commands = ['ABOUT', 'PROJECTS', 'HOBBIES', 'CONTACT', 'HELP', 'CLEAR', 'KONAMI', 'PACMAN', 'TETRIS'];
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
            case 'ABOUT':
                this.navigateToPage('about.html');
                break;
            case 'PROJECTS':
                this.navigateToPage('projects.html');
                break;
            case 'HOBBIES':
                this.navigateToPage('hobbies.html');
                break;
            case 'CONTACT':
                this.showContactInfo();
                break;
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
        const timestamp = new Date().toLocaleTimeString();
        
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
    
    navigateToPage(page) {
        this.displayOutput(`Loading ${page}...`, 'success');
        this.showLoading();
        
        setTimeout(() => {
            window.location.href = page;
        }, 1500);
    }
    
    showLoading() {
        document.getElementById('loading-section').classList.remove('hidden');
        document.getElementById('terminal-interface').style.opacity = '0.5';
    }
    
    showContactInfo() {
        this.displayOutput('');
        this.displayOutput('=== CONTACT INFORMATION ===', 'success');
        this.displayOutput('Email: developer@genxdev.com');
        this.displayOutput('GitHub: github.com/genxdev');
        this.displayOutput('LinkedIn: linkedin.com/in/genxdev');
        this.displayOutput('Ham Radio: Callsign TBD');
        this.displayOutput('');
        this.displayOutput('Preferred contact method: Email or GitHub');
        this.displayOutput('Response time: Usually within 24 hours');
    }
    
    showHelp() {
        this.displayOutput('');
        this.displayOutput('=== AVAILABLE COMMANDS ===', 'success');
        this.displayOutput('ABOUT    - View developer biography');
        this.displayOutput('PROJECTS - Browse software portfolio');
        this.displayOutput('HOBBIES  - Explore personal interests');
        this.displayOutput('CONTACT  - Display contact information');
        this.displayOutput('HELP     - Show this help message');
        this.displayOutput('CLEAR    - Clear the screen');
        this.displayOutput('DIR      - List directory contents');
        this.displayOutput('VER      - Show system version');
        this.displayOutput('TIME     - Display current time');
        this.displayOutput('DATE     - Display current date');
        this.displayOutput('');
        this.displayOutput('=== EASTER EGGS ===', 'success');
        this.displayOutput('KONAMI   - Classic cheat code');
        this.displayOutput('PACMAN   - Gaming reference');
        this.displayOutput('TETRIS   - Puzzle game tribute');
        this.displayOutput('');
        this.displayOutput('Navigation: Use arrow keys for command history');
        this.displayOutput('Auto-complete: Press TAB for command suggestions');
    }
    
    clearScreen() {
        document.getElementById('command-output').innerHTML = '';
    }
    
    showDirectory() {
        this.displayOutput('');
        this.displayOutput(' Directory of C:\\', 'success');
        this.displayOutput('');
        this.displayOutput('12/10/2025  10:00 PM    <DIR>          ABOUT');
        this.displayOutput('12/10/2025  10:00 PM    <DIR>          PROJECTS');
        this.displayOutput('12/10/2025  10:00 PM    <DIR>          HOBBIES');
        this.displayOutput('12/10/2025  10:00 PM    <DIR>          CONTACT');
        this.displayOutput('12/10/2025  10:00 PM           1337     README.TXT');
        this.displayOutput('               1 File(s)           1337 bytes');
        this.displayOutput('               4 Dir(s)    999,999,999 bytes free');
    }
    
    showVersion() {
        this.displayOutput('');
        this.displayOutput('GEN-X DEVELOPER TERMINAL', 'success');
        this.displayOutput('Version 2025.12.10');
        this.displayOutput('Built with: HTML5, CSS3, JavaScript ES6+');
        this.displayOutput('Framework: .NET C# MVC (simulated)');
        this.displayOutput('Theme: Commodore VIC-20 / Atari 2600 Retro');
        this.displayOutput('Designed for: SmarterASP.NET deployment');
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
        this.displayOutput('🎮 UP UP DOWN DOWN LEFT RIGHT LEFT RIGHT B A 🎮');
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
        this.displayOutput('🟡 . . . . 🟠 . . . . 🟡 . . . . 🟠 . . . .');
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
        this.displayOutput('🟦🟦🟦🟦🟨🟨🟨🟨🟥🟥🟥🟥🟩🟩🟩🟩');
        this.displayOutput('');
        this.displayOutput('Achievement: "Line Master" earned!');
    }
}

// Global function for HTML onclick handlers
function executeCommand(command) {
    if (window.terminal) {
        window.terminal.executeCommand(command);
    }
}

// Initialize terminal when DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
    window.terminal = new RetroTerminal();
});

// Add some retro sound effects (optional)
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

// Console easter eggs for developers
console.log(`
╔═══════════════════════════════════════╗
║     GEN-X DEVELOPER TERMINAL         ║
║     Console Easter Egg Discovered!    ║
╚═══════════════════════════════════════╝

Available console commands:
- terminal.executeCommand('command')
- playBeep(frequency, duration)
- window.terminal.showHelp()

Fun facts:
- This terminal simulates a Commodore VIC-20
- Built with vanilla JavaScript and Tailwind CSS
- Designed for .NET C# MVC deployment
- Contains multiple easter eggs

Try typing KONAMI in the terminal!
`);