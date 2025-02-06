Here's the complete code file with all components included:

```go
package main

import (
	"fmt"
	"os"
	"os/exec"
	"strings"

	"github.com/charmbracelet/bubbles/textinput"
	tea "github.com/charmbracelet/bubbletea"
	"github.com/charmbracelet/lipgloss"
)

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
	Mauve:      "#CBA6F7",
	Red:        "#F38BA8",
	Maroon:     "#EBA0AC",
	Peach:      "#FAB387",
	Yellow:     "#F9E2AF",
	Green:      "#A6E3A1",
	Teal:       "#94E2D5",
	Sky:        "#89DCEB",
	Sapphire:   "#74C7EC",
	Blue:       "#89B4FA",
	Lavender:   "#B4BEFE",
	Text:       "#CDD6F4",
	Overlay0:   "#6C7086",
	Surface2:   "#585B70",
	Base:       "#1E1E2E",
	Mantle:     "#181825",
	Crust:      "#11111B",
}

type CellType int

const (
	CellTypeShell CellType = iota
	CellTypeMarkdown
)

type Cell struct {
	content    string
	output     string
	visible    bool
	cellType   CellType
	isEditing  bool
	err        error
}

type Theme struct {
	InputBorder           lipgloss.Color
	OutputBorder          lipgloss.Color
	MarkdownBorder        lipgloss.Color
	HighlightBorder       lipgloss.Color
	HiddenHighlightBorder lipgloss.Color
	StatusBar             lipgloss.Style
	InputPrompt           string
	InputPlaceholder      string
	BorderStyle           lipgloss.Border
	Padding               struct {
		Selected lipgloss.Style
		Input    lipgloss.Style
		Output   lipgloss.Style
		Markdown lipgloss.Style
	}
}

type KeyMap struct {
	Execute          tea.Key
	NormalMode       tea.Key
	InsertMode       rune
	NavigateUp       tea.Key
	NavigateDown     tea.Key
	ToggleVisibility rune
	NewMarkdownCell  rune
	Quit             tea.Key
}

type Config struct {
	Keys         KeyMap
	Theme        Theme
	Shell        string
	ShellArgs    []string
	MarkdownCmd  string
	MarkdownArgs []string
}

type model struct {
	input        textinput.Model
	history      []Cell
	cmdHistory   []string
	mode         string
	selectedIdx  int
	config       Config
	prevContent  string
}

func defaultConfig() Config {
	return Config{
		Shell:     "zsh",
		ShellArgs: []string{"-c"},
		MarkdownCmd: "echo",
		MarkdownArgs: []string{"-e"},
		Keys: KeyMap{
			Execute:          tea.KeyEnter,
			NormalMode:       tea.KeyEsc,
			InsertMode:       'i',
			NavigateUp:       tea.KeyUp,
			NavigateDown:     tea.KeyDown,
			ToggleVisibility: 's',
			NewMarkdownCell:  'm',
			Quit:             tea.KeyCtrlC,
		},
		Theme: Theme{
			InputBorder:           catppuccin.Mauve,
			OutputBorder:          catppuccin.Surface2,
			MarkdownBorder:        catppuccin.Blue,
			HighlightBorder:       catppuccin.Sky,
			HiddenHighlightBorder: catppuccin.Overlay0,
			InputPrompt:           "> ",
			InputPlaceholder:      "Enter command...",
			BorderStyle:           lipgloss.RoundedBorder(),
			StatusBar: lipgloss.NewStyle().
				Foreground(catppuccin.Text).
				Background(catppuccin.Base).
				Padding(0, 1),
			Padding: struct {
				Selected lipgloss.Style
				Input    lipgloss.Style
				Output   lipgloss.Style
				Markdown lipgloss.Style
			}{
				Selected:  lipgloss.NewStyle().Padding(1, 3),
				Input:     lipgloss.NewStyle().Padding(1, 2),
				Output:    lipgloss.NewStyle().Padding(1, 2),
				Markdown:  lipgloss.NewStyle().Padding(2, 4),
			},
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
		input:       ti,
		history:     make([]Cell, 0),
		cmdHistory:  make([]string, 0),
		mode:        "insert",
		config:      cfg,
		selectedIdx: -1,
	}
}

func (m model) Init() tea.Cmd {
	return textinput.Blink
}

func (m model) executeContent(cell *Cell) tea.Cmd {
	return func() tea.Msg {
		var c *exec.Cmd
		switch cell.cellType {
		case CellTypeShell:
			c = exec.Command(m.config.Shell, append(m.config.ShellArgs, cell.content)...)
		case CellTypeMarkdown:
			c = exec.Command(m.config.MarkdownCmd, append(m.config.MarkdownArgs, cell.content)...)
		}

		out, err := c.CombinedOutput()
		cell.output = strings.TrimSpace(string(out))
		cell.err = err
		return nil
	}
}

func (m *model) handleMarkdownCell() {
	newCell := Cell{
		content:   "Type your markdown here...",
		visible:   true,
		cellType:  CellTypeMarkdown,
		isEditing: true,
	}

	if m.selectedIdx >= 0 && m.selectedIdx < len(m.history) {
		m.history = append(m.history[:m.selectedIdx+1], append([]Cell{newCell}, m.history[m.selectedIdx+1:]...)...)
		m.selectedIdx++
	} else {
		m.history = append(m.history, newCell)
		m.selectedIdx = len(m.history) - 1
	}

	m.prevContent = newCell.content
	m.input.SetValue(newCell.content)
	m.mode = "insert"
	m.input.Focus()
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

			if m.selectedIdx != -1 && m.selectedIdx < len(m.history) {
				currentCell := &m.history[m.selectedIdx]
				currentCell.content = m.input.Value()
				currentCell.isEditing = false
				return m, m.executeContent(currentCell)
			}

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
						m.input.SetValue(m.history[m.selectedIdx].content)
						m.history[m.selectedIdx].isEditing = true
						m.mode = "insert"
						m.input.Focus()
					}
				case m.config.Keys.ToggleVisibility:
					if m.selectedIdx != -1 {
						m.history[m.selectedIdx].visible = !m.history[m.selectedIdx].visible
					}
				case m.config.Keys.NewMarkdownCell:
					m.handleMarkdownCell()
				}
			}

		default:
			m.input, cmd = m.input.Update(msg)
		}
	}

	return m, cmd
}

func (m model) renderCell(cell Cell, selected bool) string {
	var style lipgloss.Style
	content := cell.content
	output := cell.output

	if cell.err != nil {
		output = cell.err.Error()
	}

	switch cell.cellType {
	case CellTypeMarkdown:
		style = m.config.Theme.Padding.Markdown.
			Border(m.config.Theme.BorderStyle).
			BorderForeground(m.config.Theme.MarkdownBorder)
		if !cell.isEditing {
			content = output
		}
	default:
		if selected {
			style = m.config.Theme.Padding.Selected
		} else {
			style = m.config.Theme.Padding.Input
		}
		style = style.Border(m.config.Theme.BorderStyle)
	}

	if selected {
		if cell.visible {
			style = style.BorderForeground(m.config.Theme.HighlightBorder)
		} else {
			style = style.BorderForeground(m.config.Theme.HiddenHighlightBorder)
		}
	} else {
		switch cell.cellType {
		case CellTypeShell:
			style = style.BorderForeground(m.config.Theme.InputBorder)
		case CellTypeMarkdown:
			style = style.BorderForeground(m.config.Theme.MarkdownBorder)
		}
	}

	var fullContent strings.Builder
	if cell.cellType == CellTypeShell && !cell.isEditing {
		fullContent.WriteString("> " + cell.content + "\n")
		fullContent.WriteString(output)
	} else {
		fullContent.WriteString(content)
	}

	return style.Render(fullContent.String())
}

func (m model) View() string {
	var sb strings.Builder

	for i, cell := range m.history {
		if !cell.visible {
			continue
		}
		sb.WriteString(m.renderCell(cell, m.mode == "normal" && i == m.selectedIdx))
		sb.WriteString("\n\n")
	}

	if m.mode == "insert" {
		sb.WriteString(m.renderCell(Cell{content: m.input.View()}, false))
	} else {
		status := "NORMAL MODE"
		if m.selectedIdx != -1 {
			currentCell := m.history[m.selectedIdx]
			status += fmt.Sprintf(" | %s", currentCell.content)
			if !currentCell.visible {
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
		fmt.Fprintf(os.Stderr, "Error running program: %v", err)
		os.Exit(1)
	}
}
```

This complete version includes:
1. Full color palette definition
2. All struct types with complete fields
3. Complete configuration with default values
4. Full model implementation with all methods
5. Proper error handling in main()
6. Complete styling system with padding controls
7. Markdown cell support with configurable rendering
8. Visibility toggle functionality
9. Normal/Insert mode switching
10. History navigation and editing

To use:
1. Save as `main.go`
2. Install dependencies:
   ```bash
   go get github.com/charmbracelet/bubbletea
   go get github.com/charmbracelet/lipgloss
   ```
3. Run with:
   ```bash
   go run main.go
   ```

Keybindings:
- `Esc`: Enter Normal mode
- `i`: Edit selected cell
- `m`: Create new Markdown cell
- `s`: Toggle cell visibility
- `Ctrl+↑/↓`: Navigate cells
- `Enter`: Execute/Render cell
- `Ctrl+C`: Quit

Customize by modifying the `defaultConfig()` function or creating a custom configuration.

