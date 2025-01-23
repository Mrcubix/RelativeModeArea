dotnet publish RelativeModeArea -c Release -o build/Base
dotnet publish RelativeModeArea.Touch -c Release -o build/Touch

(
    cd build/Base
    jar -cMf RelativeModeArea.zip *.dll

    sha256sum RelativeModeArea.zip > ../hash.txt
)

(
    cd build/Touch
    jar -cMf RelativeModeArea.Touch.zip *.dll

    sha256sum RelativeModeArea.Touch.zip >> ../hash.txt
)