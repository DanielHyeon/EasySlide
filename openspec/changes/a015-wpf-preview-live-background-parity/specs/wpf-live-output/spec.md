## ADDED Requirements

### Requirement: Preview and Live output SHALL use the same effective individual formatting rule

WPF Preview and Live output SHALL resolve item-specific lyrics background, font, color, and alignment from the same effective formatting rule.

#### Scenario: Individual formatting enabled carries item background to Live

GIVEN a worship item has `FormatData` with a background image code
AND `UseIndividualFormatting` is true
WHEN the operator sends the Preview item to Live
THEN Live output SHALL use the item background image instead of the default background image.

#### Scenario: Individual formatting disabled shows default formatting in Preview

GIVEN a worship item has `FormatData` with a background image code
AND `UseIndividualFormatting` is false
WHEN the operator views the item in Preview
THEN Preview SHALL show default/global background formatting
AND Preview SHALL NOT display the disabled item background as if it would be sent to Live.
