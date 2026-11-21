// MuseScore Studio 4 extension. The host supplies request as JSON in this private invocation.
function main() {
    var engraving = api.engraving;
    var score = engraving.curScore;
    if (request.operation === "inspect") {
        var notes = [];
        for (var track = 0; track < score.nstaves * 4; ++track) {
            var cursor = score.newCursor();
            cursor.track = track;
            cursor.rewind(0);
            while (cursor.segment) {
                var chord = cursor.element;
                if (chord && chord.type === engraving.Element.CHORD) {
                    var lyric = firstVerse(chord);
                    var page = cursor.measure.parent.parent;
                    for (var index = 0; index < chord.notes.length; ++index) {
                        var note = chord.notes[index];
                        notes.push({ track: track, tick: cursor.tick, index: index, pitch: note.pitch,
                            measure: cursor.measure.no + 1, lyric: lyric ? lyric.text : "",
                            page: page.pagenumber, x: note.pagePos.x / page.bbox.width,
                            y: note.pagePos.y / page.bbox.height });
                    }
                }
                cursor.next();
            }
        }
        // Only the disposable inspection copy receives this tag.
        score.setMetaTag("AxicultusInspection", JSON.stringify({ staves: score.nstaves, notes: notes }));
        return;
    }

    var at = score.newCursor();
    at.track = request.track;
    at.rewindToTick(request.tick);
    var target = at.element;
    if (!target || at.tick !== request.tick || target.type !== engraving.Element.CHORD)
        throw new Error("Chord no longer exists");
    var selected = target.notes[request.index];
    if (!selected || selected.pitch !== request.originalPitch) throw new Error("Note changed");
    score.startCmd("Edit note and lyric");
    if (selected.pitch !== request.pitch) {
        var spellings = [14, 21, 16, 11, 18, 13, 20, 15, 10, 17, 12, 19];
        var fifths = spellings[request.pitch % 12] - spellings[selected.pitch % 12];
        var concert = transposeSpelling(selected.tpc1, fifths);
        var written = transposeSpelling(selected.tpc2, fifths);
        selected.pitch = request.pitch;
        selected.tpc1 = concert;
        selected.tpc2 = written;
    }
    if (request.lyric !== null) {
        var verse = firstVerse(target);
        if (request.lyric === "") {
            if (verse) engraving.removeElement(verse);
        } else {
            var text = request.lyric.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;");
            if (verse) verse.text = text;
            else {
                verse = engraving.newElement(engraving.Element.LYRICS);
                verse.text = text;
                target.add(verse);
            }
        }
    }
    score.endCmd();
}

function firstVerse(chord) {
    for (var i = 0; i < chord.lyrics.length; ++i)
        if (chord.lyrics[i].verse === 0) return chord.lyrics[i];
    return null;
}

function transposeSpelling(tpc, fifths) {
    var result = tpc + fifths;
    while (result < 0) result += 12;
    while (result > 33) result -= 12;
    return result;
}
