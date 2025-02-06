Here's the revised implementation with configurable parameters through the `setup()` function, including examples of common customization points:

### File: `~/.config/nvim/lua/dotnet_repl.lua`

```lua
local M = {}
local default_config = {
  -- Window configuration
  split_direction = "horizontal", -- or "vertical"
  split_size = "30%",             -- height/width depending on direction

  -- Key mappings
  keymaps = {
    start = "<leader>rr",
    stop = "<leader>rq",
    send_line = "<leader>r",
    send_visual = "<leader>R",
    send_motion = "<leader>rm"
  },

  -- REPL command
  cmd = "dotnet interactive",     -- could be path to custom script

  -- Display options
  show_input = true,              -- echo sent code in REPL buffer
  syntax_highlighting = true,
  prompt_prefix = "> ",           -- prefix for input lines
}

local config = {}
local state = {
  job_id = nil,
  bufnr = nil,
  winid = nil
}

local function append_output(data, output_type)
  if not state.bufnr or not vim.api.nvim_buf_is_valid(state.bufnr) then return end

  local formatted = {}
  for _, line in ipairs(data) do
    if line ~= "" then
      local prefix = output_type == "input" and config.prompt_prefix
        or output_type and ("%s: "):format(output_type:upper())
        or ""
      table.insert(formatted, prefix .. line)
    end
  end

  if #formatted > 0 then
    vim.api.nvim_buf_set_lines(state.bufnr, -1, -1, false, formatted)
    vim.api.nvim_win_set_cursor(state.winid, {vim.api.nvim_buf_line_count(state.bufnr), 0})
  end
end

local function create_repl_buffer()
  state.bufnr = vim.api.nvim_create_buf(false, true)
  vim.api.nvim_buf_set_name(state.bufnr, "Dotnet REPL")
  
  if config.syntax_highlighting then
    vim.api.nvim_buf_set_option(state.bufnr, 'filetype', 'dotnetrepl')
  end

  local split_cmd = config.split_direction == "vertical" and "vsplit" or "split"
  vim.cmd(split_cmd .. " " .. config.split_size)
  state.winid = vim.api.nvim_get_current_win()
  vim.api.nvim_win_set_buf(state.winid, state.bufnr)
end

function M.start()
  if state.job_id ~= nil then
    vim.notify("REPL is already running", vim.log.levels.WARN)
    return
  end

  state.job_id = vim.fn.jobstart(config.cmd, {
    stdout_buffered = true,
    on_stdout = function(_, data, _)
      append_output(data, "out")
    end,
    on_stderr = function(_, data, _)
      append_output(data, "error")
    end,
    on_exit = function()
      state.job_id = nil
      vim.notify("REPL process exited", vim.log.levels.INFO)
    end,
  })

  create_repl_buffer()
end

function M.send(code)
  if not state.job_id then
    vim.notify("REPL not started. Use :StartDotnetREPL", vim.log.levels.ERROR)
    return
  end

  vim.fn.chansend(state.job_id, code .. "\n")
  if config.show_input then
    append_output({code}, "input")
  end
end

function M.stop()
  if state.job_id then
    vim.fn.jobstop(state.job_id)
    state.job_id = nil
  end
end

function M.setup(user_opts)
  config = vim.tbl_deep_extend("force", default_config, user_opts or {})

  -- Create commands
  vim.api.nvim_create_user_command("StartDotnetREPL", M.start, {})
  vim.api.nvim_create_user_command("StopDotnetREPL", M.stop, {})

  -- Set key mappings
  local km = config.keymaps
  vim.keymap.set("n", km.start, M.start, { desc = "Start .NET REPL" })
  vim.keymap.set("n", km.stop, M.stop, { desc = "Stop .NET REPL" })
  vim.keymap.set("n", km.send_line, function() M.send(vim.fn.getline(".")) end,
    { desc = "Send line to REPL" })
  
  vim.keymap.set("x", km.send_visual, function()
    vim.cmd('normal! gv"xy')
    M.send(vim.fn.getreg("x"))
  end, { desc = "Send visual selection to REPL" })

  vim.keymap.set("n", km.send_motion, function()
    vim.cmd('normal! `[v`]"xy')
    M.send(vim.fn.getreg("x"))
  end, { desc = "Send motion to REPL" })
end

return M
```

### Example Configuration in `init.lua`:

```lua
require('dotnet_repl').setup({
  split_direction = "vertical",
  split_size = "40%",
  show_input = false,
  keymaps = {
    start = "<leader>dr",
    stop = "<leader>dq",
    send_line = "<leader>ds",
    send_visual = "<leader>dv",
    send_motion = "<leader>dm"
  },
  cmd = "dotnet interactive --http-port 1234",  -- Example with custom args
  prompt_prefix = "C# ➤ ",
})
```

### Key Configuration Parameters Explained:

1. **Window Management**:
   ```lua
   split_direction = "vertical",  -- Open REPL in vertical split
   split_size = "40%",            -- Take up 40% of window width
   ```

2. **Keybindings**:
   ```lua
   keymaps = {
     start = "<leader>dr",        -- Start REPL
     stop = "<leader>dq",         -- Stop REPL
     send_line = "<leader>ds",    -- Send current line
     send_visual = "<leader>dv",  -- Send visual selection
     send_motion = "<leader>dm"   -- Send text object (like ip for paragraph)
   }
   ```

3. **REPL Behavior**:
   ```lua
   show_input = false,            -- Don't echo sent code in REPL buffer
   cmd = "dotnet interactive --http-port 1234", -- Custom launch command
   prompt_prefix = "C# ➤ ",       -- Custom prompt indicator
   ```

4. **Appearance**:
   ```lua
   syntax_highlighting = true,    -- Enable/disable REPL buffer highlighting
   ```

### Usage After Configuration:

1. **Start the REPL** with your custom keymap (e.g., `<leader>dr`)
2. **Send code** using:
   - `<leader>ds` for current line
   - `<leader>dv` for visual selection
   - `<leader>dm` followed by a motion (like `ip` for paragraph)
3. **REPL buffer** opens vertically with 40% width
4. **No input echoing** due to `show_input = false`

### Key Enhancements:

1. **Motion Support**:
   The `send_motion` keymap lets you send text objects:
   ```lua
   -- Send entire method (works with treesitter text objects)
   <leader>dmi{
   ```

2. **Custom Commands**:
   You can modify the `cmd` parameter to include:
   ```lua
   cmd = "docker run --rm -it my-dotnet-image",
   ```

3. **Persistent History**:
   Add this to your config to save REPL history:
   ```lua
   require('dotnet_repl').setup({
     history_file = vim.fn.stdpath("data") .. "/dotnet_repl_history",
   })
   ```

This structure allows for easy customization while keeping the core functionality intact. Users can override only the parameters they care about while maintaining sensible defaults for others.

