# 제안: WPF 미리보기와 Live Show 배경 적용 일치

## 배경

사도신경 항목을 미리보기에서 Live Show로 보낼 때 왼쪽 미리보기 배경이 오른쪽 Live 출력으로 그대로 넘어가지 않고 오른쪽이 default 배경을 사용하는 현상이 보고되었다.

## 문제

WPF 설정/서식 패널의 왼쪽 미리보기는 항목의 `FormatData`를 직접 읽어 배경·글자색·폰트 크기를 보여준다. 그러나 실제 Live 송출 경로는 `UseIndividualFormatting`이 꺼진 항목의 `FormatData`를 의도적으로 null 처리해 전역/default 서식을 사용한다.

이 차이 때문에 사용자는 왼쪽에서 항목 배경이 적용된 것처럼 보지만, Live Show에서는 default 배경이 출력되는 불일치를 경험할 수 있다.

## 목표

- 미리보기 샘플과 Live 송출이 같은 “유효 서식” 규칙을 사용한다.
- `Use Individual Settings`가 켜진 항목은 항목별 배경·글자색·폰트 크기가 Live Show로 전달된다.
- `Use Individual Settings`가 꺼진 항목은 왼쪽 미리보기에서도 default 서식으로 보여, Live Show와 다르게 보이지 않는다.
- 구현 후 사도신경으로 실제 앱 캡처를 남긴다.

## 비목표

- 레거시 `FormatData` 코드 체계 전체를 재설계하지 않는다.
- OutputWindow 렌더러의 전역 배경 우선순위를 변경하지 않는다.
- PowerPoint/미디어 송출 경로는 이번 범위에서 변경하지 않는다.

## 수용 기준

- GIVEN 사도신경 항목에 항목별 배경 `61=`이 있고 `UseIndividualFormatting=true`일 때, WHEN `Preview Go Live`를 실행하면 THEN Live 세션과 출력 화면은 해당 항목 배경을 사용해야 한다.
- GIVEN 사도신경 항목에 항목별 배경 `61=`이 있어도 `UseIndividualFormatting=false`일 때, WHEN 미리보기 샘플을 보면 THEN 항목 배경이 아니라 default 배경을 보여야 한다.
- GIVEN `UseIndividualFormatting=false`일 때, WHEN Live Show로 보내면 THEN 오른쪽이 default 배경을 쓰는 동작과 왼쪽 미리보기가 일치해야 한다.
