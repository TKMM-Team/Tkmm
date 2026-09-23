#!/bin/sh
set -eu

cd "$(dirname "$0")/../.."
ROOT="$(pwd)"

RID="${1:-}"
case "$RID" in
    linux-x64)
        ARCH_NAME=x64
        export ARCH=x86_64
        ;;
    linux-arm64)
        ARCH_NAME=arm64
        export ARCH=aarch64
        ;;
    *)
        echo "Usage: $0 linux-x64|linux-arm64"
        exit 1
        ;;
esac

publish_dir="${ROOT}/Tkmm-${RID}"

mkdir -p "${ROOT}/tools"
if [ ! -f "${ROOT}/tools/appimagetool" ]; then
    wget -q -O "${ROOT}/tools/appimagetool" \
        "https://github.com/AppImage/appimagetool/releases/download/continuous/appimagetool-x86_64.AppImage"
    chmod +x "${ROOT}/tools/appimagetool"
fi

rm -rf "${ROOT}/AppDir"
mkdir -p \
    "${ROOT}/AppDir/usr/bin" \
    "${ROOT}/AppDir/usr/share/applications" \
    "${ROOT}/AppDir/usr/share/icons/hicolor/scalable/apps"

cp distribution/appimage/Tkmm.desktop "${ROOT}/AppDir/usr/share/applications/tkmm.desktop"
ln -s usr/share/applications/tkmm.desktop "${ROOT}/AppDir/Tkmm.desktop"
cp distribution/appimage/AppRun "${ROOT}/AppDir/AppRun"
cp distribution/appimage/tkmm.svg "${ROOT}/AppDir/tkmm.svg"
cp distribution/appimage/tkmm.svg "${ROOT}/AppDir/usr/share/icons/hicolor/scalable/apps/tkmm.svg"
cp -R "${publish_dir}/." "${ROOT}/AppDir/usr/bin/"
chmod +x "${ROOT}/AppDir/AppRun" "${ROOT}/AppDir/usr/bin/Tkmm*"

export UFLAG="gh-releases-zsync|${GITHUB_REPOSITORY_OWNER:-}|${GITHUB_REPOSITORY##*/}|latest|*-${ARCH_NAME}.AppImage.zsync"

APPIMAGE_EXTRACT_AND_RUN=1 "${ROOT}/tools/appimagetool" \
    --comp zstd --mksquashfs-opt -Xcompression-level --mksquashfs-opt 21 \
    -u "$UFLAG" "${ROOT}/AppDir"
mv "${ROOT}/"*.AppImage "${ROOT}/Tkmm-${RID}.AppImage"
