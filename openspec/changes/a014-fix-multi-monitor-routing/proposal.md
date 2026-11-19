# Proposal

## Why

운영자가 배치한 창과 영상이 다른 화면으로 이동하거나 스테이지 화면이 관객 출력을 가릴 수 있다.

## What Changes

WinForms의 미디어 장치 선택, PowerPoint 수동 좌표와 운영 창 복원을 수정한다. WPF의 관객·스테이지 배치를 분리하고 장치 변경 및 화면 배율을 반영한다.

## Capabilities

### New Capabilities

- `multi-monitor-routing`은 두 셸의 모니터 선택과 실제 창 배치를 다룬다.

## Impact

WinForms 라이브 송출, WPF 디스플레이 서비스와 창, 두 테스트 프로젝트에 적용한다.
