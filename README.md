# BeatLeader PC mod

An open-source leaderboard System for Beat Saber
- [Join our Discord server](https://discord.gg/2RG5YVqtG6)
- [Support us on Patreon](https://www.patreon.com/beatleader)

![cover](./Media/cover.png)

## Usage

- Download zip for your game version from the [Releases](https://github.com/BeatLeader/beatleader-mod/releases) and extract it to your BeatSaber directory
- Make sure to install and update all required [dependencies](#dependencies)

Go to the https://beatleader.xyz/ to see your scores on the web

If you experience any issues, have any suggestions or bug reports - you can leave them in our [Discord server](https://discord.gg/2RG5YVqtG6)

## Dependencies

- BSIPA, BSML, SiraUtil - available in [ModAssistant](https://github.com/Assistant/ModAssistant/releases/latest) and on the [BeatMods website](https://beatmods.com/#/mods)
- [LeaderboardCore](https://github.com/rithik-b/LeaderboardCore)


## Development

- Clone the repository
- Create the `BeatLeader.csproj.user` file in the `Source` directory with the following content (do not forget to fill variables with your own values):
```xml
<Project>
    <PropertyGroup>
        <!-- Beat Saber 1.33.0 -->
        <BeatSaber1330Dir><!--...\Beat Saber--></BeatSaber1330Dir>
        <BeatSaber1330RefPath><!--...\Beat Saber\Plugins-->;<!--...\Beat Saber\Beat Saber_Data\Managed--></BeatSaber1330RefPath>
        <!-- Beat Saber 1.29.1 -->
        <BeatSaber1291Dir><!--...\Beat Saber--></BeatSaber1291Dir>
        <BeatSaber1291RefPath><!--...\Beat Saber\Plugins-->;<!--...\Beat Saber\Beat Saber_Data\Managed--></BeatSaber1291RefPath>
    </PropertyGroup>
</Project>
```
- Go to the IDE and select the proper configuration (`Release 1.33.0` for the master branch)
- Click compile and that's it!