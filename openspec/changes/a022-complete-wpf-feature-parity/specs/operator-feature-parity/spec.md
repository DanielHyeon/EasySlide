## ADDED Requirements

### Requirement: Main window commands reach their operator functions
The WPF operator console SHALL route menu and toolbar actions to their corresponding library, media, alert and output operations.

#### Scenario: Change Chinese script on Output
- **WHEN** the operator toggles Chinese script on a prepared or live output item
- **THEN** the output changes script while the original source and bilingual region selection are preserved

#### Scenario: Configure a custom widescreen monitor
- **WHEN** the operator chooses widescreen for a custom audience output
- **THEN** output height is calculated from the custom width using 16:9

### Requirement: Preserve paragraphs on text file slides
The console SHALL support fitting a complete external text-file paragraph on a single slide and display the same page on Preview and Output.

#### Scenario: Read an external text with a blank separator
- **WHEN** paragraph fitting is enabled and the operator advances a text file
- **THEN** the following paragraph appears as a single page without truncating its contents


### Requirement: Route source edits and retain existing presentation settings
The Edit menu and main toolbar SHALL act on the active source selection. Applying session defaults SHALL preserve an information screen's individual formatting.

#### Scenario: Copy the selected PowerPoint
- **WHEN** PowerPoint is the active source and the operator chooses Copy from Edit or the toolbar
- **THEN** the file operation targets the selected presentation

#### Scenario: Open a saved session template
- **WHEN** the operator opens a WinForms session .est file
- **THEN** the console reads the supported display and media preferences together and retains unrecognized header fields for a subsequent export

### Requirement: Control audience alerts and assigned media
The console SHALL display audience and parental announcements independently, restore their separate histories, and apply item media settings during output playback.

#### Scenario: Black out an active announcement
- **WHEN** the operator blacks out the audience screen while a message and parental call are active
- **THEN** both banners disappear and their active messages can be restored with the content

#### Scenario: Play an item with media settings
- **WHEN** an operator plays an item with a saved media source, volume, repeat flag and output display
- **THEN** the player uses the saved options and reports the decoder's playback position, completion or error


#### Scenario: Change one language's display settings
- **WHEN** an operator edits the secondary-language font or toggles notation or transpose for one item
- **THEN** the prepared output uses the item's settings and other media and presentation fields remain available when the queue is saved

#### Scenario: Work in a source subfolder
- **WHEN** an operator imports files from the information screen, media or presentation source pane
- **THEN** the import uses the selected folder and the source list refreshes

#### Scenario: Decode a local file with installed DirectShow filters
- **WHEN** the primary WPF media player fails to decode an assigned local video that an installed DirectShow filter can render
- **THEN** the player presents decoded frames within the WPF output surface, reports playback position and completion, and releases the graph when the file is unloaded
