## Context

Both desktop applications exchange songs through files. Their UI and database models differ, and the WPF XML preview currently builds a complete song collection.

## Goals / Non-Goals

Support XML and JSON song collections in the two applications. Worship lists and info screens continue to use their existing formats.

## Decisions

JSON is a UTF-8 array of song objects. Field names follow the XML song fields, with integer SongNumber and Capo and textual verse sequences. Clients use a shared reader and writer. JSON exports replace the destination after writing a complete temporary file. Folder previews project only the folder field. XML previews skip unrelated elements and large values.

## Risks / Trade-offs

A full import validates the input before database writes. WinForms scans the same read-locked file twice to bound memory. WPF builds its existing write-model collection. WinForms reports unsupported section labels before saving any selected song. Tests cover empty XML items, missing fields, folder suffix collisions and malformed files.
