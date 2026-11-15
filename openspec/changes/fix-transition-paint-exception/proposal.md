# Proposal: 화면 전환을 그릴 때 나는 예외 수정

## Why

송출 단에 글자 항목이 있는 상태에서 검은 화면이나 글자 숨김을 누르면, 큰 송출 화면에 빨간 X가 뜨고 "Unhandled exception … Parameter is not valid" 창이 나온다. 이 창이 떠 있는 동안 메인 화면은 눌리지 않는다.

원인은 `ImageTransitionControl.OnPaint`다. 생성자에서 한 번만 만드는 `_cachedImageAttributes`를 전환 효과를 그릴 때마다 `Dispose()`한다. 첫 프레임 뒤에는 이미 해제된 객체에 `SetColorMatrix`를 부르게 된다. 커밋 `e98ad3e`(2025-12-29, 성능 개선)가 매번 새로 만들던 객체를 필드로 바꾸면서 해제 호출을 그대로 남겼다.

- 같은 소스를 .NET 10으로 빌드해도 똑같이 난다. .NET 11과 무관하다.
- 같은 코드가 송출 창(`FrmLaunchShow`)의 화면에도 쓰인다. Fade, Ascend, Descend 등 이 분기를 쓰는 전환은 프로젝터 화면에서도 같은 예외를 낸다.

## What Changes

- `OnPaint` 안의 `_cachedImageAttributes.Dispose()` 호출 8곳을 없앤다. 컨트롤이 해제될 때의 `Dispose(bool)` 호출은 남긴다.

## Out of Scope

- 전환 효과의 모양이나 속도
- `ValidateWorshipListItems`의 안내 창. 쇼가 시작되지 않던 것은 예배 목록에 없는 PPT 파일이 있어서였고, 코드 문제가 아니다.

## Impact

- `Easislides/Easislides/ImageTransitionControl.cs` 한 파일
