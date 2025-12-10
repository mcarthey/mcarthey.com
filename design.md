# Retro Computing Personal Website - Design Style Guide

## Design Philosophy

### Color Palette
- **Primary Green**: #00FF00 (phosphor green - authentic CRT terminal color)
- **Dark Green**: #008000 (dimmed phosphor for secondary text)
- **Black**: #000000 (deep space black for backgrounds)
- **Amber**: #FFB000 (warm accent for highlights and active states)
- **Grey**: #808080 (for disabled/inactive elements)

### Typography
- **Display Font**: 'Courier New', monospace (authentic terminal font)
- **Body Font**: 'Courier New', monospace (consistent terminal aesthetic)
- **Pixel Font**: 'Press Start 2P' for gaming elements (Google Fonts)
- **Font Sizes**: Small and crisp to mimic CRT display limitations

### Visual Language
- **CRT Monitor Aesthetic**: Rounded corners, subtle bezel effects, screen curvature simulation
- **Phosphor Glow**: Green text with subtle glow effects using CSS text-shadow
- **Scan Lines**: Subtle horizontal lines overlay to simulate CRT scanlines
- **Pixel Perfect**: All elements align to 8px grid for authentic pixel art feel
- **Monochromatic**: Primarily green-on-black with strategic amber accents

## Visual Effects

### Used Libraries
- **Anime.js**: For smooth terminal cursor blinking and text typing animations
- **p5.js**: For CRT noise/static effects and particle systems
- **Splitting.js**: For character-by-character text reveal animations
- **Typed.js**: For authentic terminal typing effects
- **Pixi.js**: For advanced visual effects like screen flicker and distortion

### Animation Effects
- **Boot Sequence**: Authentic computer startup with memory check and loading
- **Typing Animation**: Character-by-character text reveal with cursor following
- **Screen Flicker**: Subtle CRT-style screen refresh simulation
- **Scanline Sweep**: Moving horizontal line effect during transitions
- **Phosphor Persistence**: Text that briefly remains after being overwritten

### Header Effects
- **CRT Monitor Frame**: Realistic bezel with rounded corners and depth
- **Power LED**: Small amber dot that glows when "powered on"
- **Screen Reflection**: Subtle gradient overlay to simulate glass surface
- **Static Noise**: Brief static effect during page transitions

### Interactive Elements
- **Terminal Prompt**: Blinking cursor with command input simulation
- **Command History**: Arrow key navigation through previous commands
- **Error Messages**: Authentic DOS-style error formatting
- **Loading Bars**: Retro progress indicators with block characters
- **Hover States**: Subtle glow effects on interactive elements

### Styling Approach
- **8-bit Inspired**: All icons and graphics use pixel art aesthetic
- **Authentic Constraints**: Limited color palette and resolution simulation
- **Terminal Realism**: Command-line interface as primary navigation
- **Gaming References**: Easter eggs and hidden commands for classic games
- **Gen X Nostalgia**: References to 80s computing culture and aesthetics

### Background Treatment
- **Solid Black**: Deep space black (#000000) throughout all pages
- **Subtle Texture**: Very light CRT phosphor texture overlay
- **No Gradients**: Maintains authentic flat display appearance
- **Consistent Theme**: Same background treatment across all sections

### Responsive Design
- **Desktop First**: Optimized for larger screens like actual CRT monitors
- **Mobile Adaptation**: Touch-friendly ASCII art buttons for smaller screens
- **Keyboard Navigation**: Full keyboard support for authentic terminal feel
- **Progressive Enhancement**: Works without JavaScript but enhanced with it

This design creates an immersive retro computing experience that honors the VIC-20 and Atari 2600 era while providing modern functionality and interactivity.