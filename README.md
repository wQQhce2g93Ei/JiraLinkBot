# JiraLinkBot
A simple bot that listens to Slack channels and tries to find Jira tickets and replies with a link. JiraLinkBot is based on the [Noobot](https://github.com/noobot/noobot) project using Noobot.Runner as the starting codebase.

## Usage
1. Clone the repository: `git clone https://github.com/wQQhce2g93Ei/JiraLinkBot.git`
1. Build but don't run the solution (this generates the `configuration\config.json` file)
1. Go to your Team's Slack [bot integration page](https://my.slack.com/services/new/bot).
1. Enter 'jiralinkbot' as the Username and click the 'Add Bot Integration' button.
1. Copy the generated API key and paste it as your `slack` `apiToken` in `configuration\config.json`:
  ```
    "slack": {
      "apiToken": "xxxx-00000000000-xxxxxxxxxxxxxxxxxxxxxxxx"
    }    
  ```
6. Build and run the JiraLinkBot solution.
7. Invite @jiralinkbot to a channel and add the Jira prefixs for your projects
  ```
    /invite @jiralinkbot
    @jiralinkbot addproject PROJ
  ```

You should now be able to send messages to JiraLinkBot in your Team's Slack. Try direct messaging with `@jiralinkbot help` to see a list of commands.

To extend the JiraLinkBot refer to the [Noobot documentation](https://github.com/noobot/noobot/wiki).

To install as a service run these commands

```
cd JiralinkBot\bin\Release\
.\JiraLinkBot.exe install
.\JiraLinkBot.exe start 
```
