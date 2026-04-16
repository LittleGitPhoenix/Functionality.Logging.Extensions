# Seq Integration Tests Configuration

## Environment Variables

The integration tests require the following environment variables to be set:

| Variable Name | Description | Example |
|--------------|-------------|---------|
| `SEQ_HOST` | The URL of your Seq server | `https://seq.example.com` |
| `SEQ_PORT` | The port number of your Seq server | `5341` |
| `SEQ_CONFIGURATION_API_KEY` | The API key for managing Seq configuration | `•••` |

## Setting Environment Variables

### Windows (PowerShell)

```powershell
$env:SEQ_HOST = "https://your-seq-server.example.com"
$env:SEQ_PORT = "5341"
$env:SEQ_CONFIGURATION_API_KEY = "•••"
```

### Windows (Command Prompt)

```cmd
set SEQ_HOST=https://your-seq-server.example.com
set SEQ_PORT=5341
set SEQ_CONFIGURATION_API_KEY=•••
```

### Linux/macOS

```bash
export SEQ_HOST="https://your-seq-server.example.com"
export SEQ_PORT="5341"
export SEQ_CONFIGURATION_API_KEY="•••"
```

### Visual Studio

1. Right-click on the test project → **Properties**
2. Go to **Debug** → **General**
3. Click **Open debug launch profiles UI**
4. Add environment variables in the **Environment variables** section

### Rider

1. Go to **Run** → **Edit Configurations**
2. Select your test configuration
3. Add environment variables in the **Environment variables** field

## CI/CD Integration

### GitHub Actions

Add secrets to your repository and reference them in your workflow:

```yaml
- name: Run Tests
  env:
    SEQ_HOST: ${{ secrets.SEQ_HOST }}
    SEQ_PORT: ${{ secrets.SEQ_PORT }}
    SEQ_CONFIGURATION_API_KEY: ${{ secrets.SEQ_CONFIGURATION_API_KEY }}
  run: dotnet test
```

### Azure DevOps

Add variables to your pipeline (mark them as secret):

```yaml
variables:
  SEQ_HOST: $(SeqHost)
  SEQ_PORT: $(SeqPort)
  SEQ_CONFIGURATION_API_KEY: $(SeqConfigurationApiKey)
```

## Default Values

If environment variables are not set, the tests will use placeholder values:
- **SEQ_HOST:** `https://your-seq-server.example.com`
- **SEQ_PORT:** `5341`
- **SEQ_CONFIGURATION_API_KEY:** `your-configuration-api-key-here`

Tests will likely fail with these defaults unless you have a server at that address.
