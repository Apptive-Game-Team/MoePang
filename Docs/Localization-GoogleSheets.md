# Google Sheets 번역 가져오기

1. 팀에서 안전하게 전달받은 OAuth 클라이언트 JSON을 프로젝트 루트 기준
   `UserSettings/Localization/google-oauth.json`에 저장합니다.
   Google Cloud에서 내려받은 `installed` 또는 `web` 형식의 JSON을 사용합니다.
   `UserSettings/`는 Git에서 제외됩니다. 계정 비밀번호나 로그인 토큰을 공유하지 않습니다.
2. Unity에서 `Debug Tool > Pull Google Sheets Localization Data`를 실행합니다.
3. 최초 실행 시 시트 접근 권한이 있는 본인의 Google 계정으로 로그인합니다.
4. 변경된 번역 테이블 에셋을 검토하고 저장한 뒤 커밋합니다.

`Assets/Localization/Google Sheets Service.asset`과 `.meta`는 인증 정보 없이
공유합니다. 시트 ID와 열 매핑은 `LocalizationDataTable` 컬렉션에서 관리합니다.
Pull 코드는 Provider를 메모리에서 복제한 뒤 로컬 JSON의 인증 정보를 주입하므로,
공용 Provider의 Inspector에서 Client Secret을 입력하거나 Load Credentials를 실행할 필요가 없습니다.
이 로컬 인증 처리는 위 Debug Tool 메뉴에 적용됩니다. Localization Tables 창의 기본 Pull/Push는
빈 공용 Provider에 인증 정보가 없으므로 사용할 수 없습니다.

토큰은 Unity Provider 기본 위치인 `Library/Google/Google Sheets Service`에 저장됩니다.
일반적인 병합 후에는 JSON과 토큰을 재사용하며, Library 삭제나 인증 만료 시에는 다시 로그인할 수 있습니다.
시트에서 삭제된 키를 제거하는 기존 Pull 옵션은 그대로 적용됩니다.

JSON이 없거나 잘못되면 콘솔에 안내가 표시되며, JSON 내용은 로그에 출력하지 않습니다.
Secret이 없는 Provider 에셋과 `.meta`, 컬렉션 참조, 에디터 코드는 함께 커밋합니다.
