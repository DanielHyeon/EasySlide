## Decisions

Carry search SQL together with its named parameter values when a search is saved and sorted again. Generate syntax only from application-owned selections and integer sets. Quote SQLite identifiers independently of value binding. Continue to use the existing provider and database schemas.

Stream each song listing once. Match both merge folders in one query, preserving LIKE semantics and duplicate matches. Execute selected usage deletions with one connection, one reusable command and one transaction. Validate on isolated SQLite data.

SQLite file paths are encoded with connection string builders. Import sources open read-only and schema discovery reads actual column names through pragma_table_info. Whole-word Bible queries pad the verse boundaries and use bound GLOB patterns. The existing LIKE wildcards are translated when forming those patterns. Reading only the six list columns leaves larger lyric and format fields out of folder refreshes.
