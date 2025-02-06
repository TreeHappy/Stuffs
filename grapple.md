Here's the enhanced version with visibility toggling and full customization support:

```go
package main

import (
	"os"
	"os/exec"
	"strings"

	"github.com/charmbracelet/bubbles/textinput"
	tea "github.com/charmbracelet/bubbletea"
	"github.com/charmbracelet/lipgloss"
)

// Catppuccin Mocha color palette
var catppuccin = struct {
	Rosewater  lipgloss.Color
	Flamingo   lipgloss.Color
	Pink       lipgloss.Color
	Mauve      lipgloss.Color
	Red        lipgloss.Color
	Maroon     lipgloss.Color
	Peach      lipgloss.Color
	Yellow     lipgloss.Color
	Green      lipgloss.Color
	Teal       lipgloss.Color
	Sky        lipgloss.Color
	Sapphire   lipgloss.Color
	Blue       lipgloss.Color
	Lavender   lipgloss.Color
	Text       lipgloss.Color
	Overlay0   lipgloss.Color
	Surface2   lipgloss.Color
	Base       lipgloss.Color
	Mantle     lipgloss.Color
	Crust      lipgloss.Color
}{
	Rosewater:  "#F5E0DC",
	Flamingo:   "#F2CDCD",
	Pink:       "#F5C2E7",
	Mauve:     "#CBA6F7",
	Red:       "#F38BA8",
	Maroon:    "#EBA0AC",
	Peach:     "#FAB387",
	Yellow:    "#F9E2AF",
	Green:     "#A6E3A1",
	Teal:      "#94E2D5",
	Sky:       "#89DCEB",
	Sapphire:  "#74C7EC",
	Blue:      "#89B4FA",
	Lavender:  "#B4BEFE",
	Text:      "#CDD6F4",
	Overlay0:  "#6C7086",
	Surface2:  "#585B70",
	Base:      "#1E1E2E",
	Mantle:    "#181825",
	Crust:     "#11111B",
}

type Theme struct {
	InputBorder        lipgloss.Color
	OutputBorder       lipgloss.Color
	HighlightBorder    lipgloss.Color
	StatusBar          lipgloss.Style
	InputPrompt        string
	InputPlaceholder   string
	BorderStyle        lipgloss.Border
}

type KeyMap struct {
	Execute          tea.Key
	NormalMode       tea.Key
	InsertMode       rune
	NavigateUp       tea.Key
	NavigateDown     tea.Key
	ToggleVisibility rune
	Quit             tea.Key
}

type Config struct {
	Keys      KeyMap
	Theme     Theme
	Shell     string
	ShellArgs []string
}

type msgExecResult struct {
	cmd        string
	output     string
	err        error
	replaceIdx int
	visible    bool
}

type model struct {
	input       textinput.Model
	history     []msgExecResult
	cmdHistory  []string
	mode        string
	selectedIdx int
	config      Config
}

func defaultConfig() Config {
	return Config{
		Shell:     "zsh",
		ShellArgs: []string{"-c"},
		Keys: KeyMap{
			Execute:          tea.KeyEnter,
			NormalMode:       tea.KeyEsc,
			InsertMode:       'i',
			NavigateUp:       tea.KeyUp,
			NavigateDown:     tea.KeyDown,
			ToggleVisibility: 's',
			Quit:             tea.KeyCtrlC,
		},
		Theme: Theme{
			InputBorder:      catppuccin.Mauve,
			OutputBorder:     catppuccin.Surface2,
			HighlightBorder:  catppuccin.Sky,
			InputPrompt:      "> ",
			InputPlaceholder: "Enter command...",
			BorderStyle:      lipgloss.RoundedBorder(),
			StatusBar: lipgloss.NewStyle().
				Foreground(catppuccin.Text).
				Background(catppuccin.Base).
				Padding(0, 1),
		},
	}
}

func initialModel(cfg Config) model {
	ti := textinput.New()
	ti.Prompt = cfg.Theme.InputPrompt
	ti.Placeholder = cfg.Theme.InputPlaceholder
	ti.PromptStyle = ti.PromptStyle.Foreground(catppuccin.Text)
	ti.TextStyle = ti.TextStyle.Foreground(catppuccin.Text)
	ti.Focus()

	return model{
		input:      ti,
		history:    make([]msgExecResult, 0),
		cmdHistory: make([]string, 0),
		mode:       "insert",
		config:     cfg,
	}
}

func (m model) Init() tea.Cmd {
	return textinput.Blink
}

func (m model) executeCommand(cmd string, replaceIdx int) tea.Cmd {
	return func() tea.Msg {
		c := exec.Command(m.config.Shell, append(m.config.ShellArgs, cmd)...)
		out, err := c.CombinedOutput()
		return msgExecResult{
			cmd:        cmd,
			output:     strings.TrimSpace(string(out)),
			err:        err,
			replaceIdx: replaceIdx,
			visible:    true,
		}
	}
}

func (m model) Update(msg tea.Msg) (tea.Model, tea.Cmd) {
	var cmd tea.Cmd

	switch msg := msg.(type) {
	case tea.KeyMsg:
		switch {
		case msg.Type == m.config.Keys.Quit.Type:
			return m, tea.Quit

		case msg.Type == m.config.Keys.NormalMode:
			m.mode = "normal"
			m.input.Blur()
			if m.selectedIdx == -1 && len(m.history) > 0 {
				m.selectedIdx = len(m.history) - 1
			}
			return m, nil

		case msg.Type == m.config.Keys.Execute && m.mode == "insert":
			if m.input.Value() == "" {
				return m, nil
			}
			replaceIdx := -1
			if m.selectedIdx != -1 {
				replaceIdx = m.selectedIdx
			}
			m.cmdHistory = append(m.cmdHistory, m.input.Value())
			execCmd := m.executeCommand(m.input.Value(), replaceIdx)
			m.input.Reset()
			if replaceIdx == -1 {
				m.selectedIdx = -1
			}
			return m, execCmd

		case m.mode == "normal":
			switch {
			case msg.Type == m.config.Keys.NavigateUp:
				if m.selectedIdx > 0 {
					m.selectedIdx--
				}
			case msg.Type == m.config.Keys.NavigateDown:
				if m.selectedIdx < len(m.history)-1 {
					m.selectedIdx++
				}
			case msg.Runes != nil && len(msg.Runes) > 0:
				switch msg.Runes[0] {
				case m.config.Keys.InsertMode:
					if m.selectedIdx != -1 {
						m.input.SetValue(m.history[m.selectedIdx].cmd)
					}
					m.mode = "insert"
					m.input.Focus()
				case m.config.Keys.ToggleVisibility:
					if m.selectedIdx != -1 {
						m.history[m.selectedIdx].visible = !m.history[m.selectedIdx].visible
					}
				}
			}

		default:
			m.input, cmd = m.input.Update(msg)
		}

	case msgExecResult:
		if msg.replaceIdx != -1 && msg.replaceIdx < len(m.history) {
			m.history[msg.replaceIdx] = msg
		} else {
			m.history = append(m.history, msg)
			m.selectedIdx = len(m.history) - 1
		}
	}

	return m, cmd
}

func (m model) renderBox(content string, isInput bool, selected bool) string {
	if selected {
		return lipgloss.NewStyle().
			Border(m.config.Theme.BorderStyle).
			BorderForeground(m.config.Theme.HighlightBorder).
			Padding(1, 2).
			Margin(1, 0).
			Render(content)
	}

	if isInput {
		return lipgloss.NewStyle().
			Border(m.config.Theme.BorderStyle).
			BorderForeground(m.config.Theme.InputBorder).
			Padding(1, 2).
			Margin(1, 0).
			Render(content)
	}

	return lipgloss.NewStyle().
		Border(m.config.Theme.BorderStyle).
		BorderForeground(m.config.Theme.OutputBorder).
		Padding(1, 2).
		Margin(1, 0).
		Render(content)
}

func (m model) View() string {
	var sb strings.Builder

	for i, result := range m.history {
		if !result.visible {
			continue
		}
		selected := m.mode == "normal" && i == m.selectedIdx
		sb.WriteString(m.renderBox("> "+result.cmd, true, selected) + "\n")
		
		output := result.output
		if result.err != nil {
			output = result.err.Error()
		}
		sb.WriteString(m.renderBox(output, false, selected) + "\n\n")
	}

	if m.mode == "insert" {
		sb.WriteString(m.renderBox(m.input.View(), true, false))
	} else {
		status := "NORMAL MODE"
		if m.selectedIdx != -1 {
			status += " | Selected: " + m.history[m.selectedIdx].cmd
			if !m.history[m.selectedIdx].visible {
				status += " (hidden)"
			}
		}
		sb.WriteString(m.config.Theme.StatusBar.Render(status))
	}

	return sb.String()
}

func main() {
	cfg := defaultConfig()
	p := tea.NewProgram(initialModel(cfg), tea.WithAltScreen())
	if _, err := p.Run(); err != nil {
		panic(err)
		os.Exit(1)
	}
}
```

Key additions:
1. Added `ToggleVisibility` key binding (default 's')
2. Each history entry has a `visible` boolean flag
3. Pressing 's' in normal mode toggles visibility of selected cell
4. Hidden cells are not rendered but remain in history
5. Navigation works through hidden cells
6. Status bar shows if selected cell is hidden
7. New commands and re-run commands are always visible

Usage:
- In normal mode (Esc):
  - Ctrl+↑/↓ to navigate cells
  - 's' to toggle visibility of selected cell
  - 'i' to edit selected cell's command
- Hidden cells are preserved but not rendered
- Selected hidden cells show "(hidden)" in status bar
- Re-running a command makes it visible again

Customization:
- Change toggle key in config: `cfg.Keys.ToggleVisibility = 'v'`
- Modify visibility behavior through the `visible` flag in history entries
- Adjust visual presentation in `renderBox` method

