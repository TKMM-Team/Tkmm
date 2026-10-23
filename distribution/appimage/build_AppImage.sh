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
VERSION="${VERSION:-1.0.0}"

mkdir -p "${publish_dir}"
rm -rf $ROOT/*.AppImage
rm -rf $ROOT/AppDir

dotnet publish src/Tkmm \
    -c Release \
    -o "${publish_dir}" \
    -r "${RID}" \
    --sc true \
    --version-suffix "${VERSION}" \
    -p:Version="${VERSION}" \
    -p:DebugType=embedded \
    -p:UseApplicationDataFolder=true

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
chmod +x "${ROOT}/AppDir/AppRun" "${ROOT}"/AppDir/usr/bin/Tkmm*

if [ -z "${GITHUB_REPOSITORY:-}" ] || [ -z "${GITHUB_REPOSITORY_OWNER:-}" ]; then
    origin_url="$(git remote get-url origin 2>/dev/null || true)"
    origin_path="${origin_url#*github.com[:/]}"
    origin_path="${origin_path%.git}"
    origin_owner="${origin_path%%/*}"
    origin_repo="${origin_path#*/}"

    if [ -z "${GITHUB_REPOSITORY:-}" ] && [ -n "${origin_owner}" ] && [ -n "${origin_repo}" ]; then
        GITHUB_REPOSITORY="${origin_owner}/${origin_repo}"
    fi
    if [ -z "${GITHUB_REPOSITORY_OWNER:-}" ] && [ -n "${origin_owner}" ]; then
        GITHUB_REPOSITORY_OWNER="${origin_owner}"
    fi
fi

export UFLAG="gh-releases-zsync|${GITHUB_REPOSITORY_OWNER:-}|${GITHUB_REPOSITORY##*/}|latest|*-${ARCH_NAME}.AppImage.zsync"

APPIMAGE_EXTRACT_AND_RUN=1 "${ROOT}/tools/appimagetool" \
    --comp zstd --mksquashfs-opt -Xcompression-level --mksquashfs-opt 21 \
    -u "$UFLAG" "${ROOT}/AppDir"
mv "${ROOT}/"*.AppImage "${ROOT}/Tkmm-${RID}.AppImage"
