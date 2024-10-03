dotnet publish src -c Release -o build

(
    cd build
    jar -cMf RelativeModeArea.zip RelativeModeArea.dll

    sha256sum RelativeModeArea.zip > hash.txt
)