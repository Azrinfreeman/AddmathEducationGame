"""Export this tutorial's simple Markdown using Python's standard library only."""
from pathlib import Path
import html
import re

ROOT = Path(__file__).resolve().parent
SOURCE = ROOT / "kids-ui-revamp.md"


def inline(value):
    value = html.escape(value)
    value = re.sub(r"`([^`]+)`", r"<code>\1</code>", value)
    value = re.sub(r"\*\*([^*]+)\*\*", r"<strong>\1</strong>", value)
    return re.sub(r"\[([^]]+)\]\(([^)]+)\)", r'<a href="\2">\1</a>', value)


def export():
    blocks = []
    paragraph = []
    code = []
    in_code = False
    list_type = None

    def flush():
        if paragraph:
            blocks.append("<p>" + inline(" ".join(paragraph)) + "</p>")
            paragraph.clear()

    for line in SOURCE.read_text(encoding="utf-8").splitlines():
        if line.startswith("```"):
            flush()
            if in_code:
                blocks.append("<pre><code>" + html.escape("\n".join(code)) + "</code></pre>")
                code.clear()
            in_code = not in_code
            continue
        if in_code:
            code.append(line)
            continue
        item = re.match(r"^(- |\d+\. )(.*)", line)
        kind = "ul" if line.startswith("- ") else "ol"
        if list_type and (not item or list_type != kind):
            blocks.append(f"</{list_type}>")
            list_type = None
        if item:
            flush()
            if not list_type:
                list_type = kind
                blocks.append(f"<{kind}>")
            blocks.append("<li>" + inline(item[2]) + "</li>")
        elif line.startswith("#"):
            flush()
            level = len(line) - len(line.lstrip("#"))
            title = line[level:].strip()
            anchor = re.sub(r"[^a-z0-9]+", "-", title.lower()).strip("-")
            blocks.append(f'<h{level} id="{anchor}">' + inline(title) + f"</h{level}>")
        elif not line.strip():
            flush()
        else:
            paragraph.append(line)
    flush()
    if list_type:
        blocks.append(f"</{list_type}>")
    style = """body{max-width:70ch;margin:3rem auto;padding:0 1.5rem;color:#203a47;
font:17px/1.55 system-ui,sans-serif;background:#fffdf7}h1{font-size:2.2rem;line-height:1.15}
h2{font-size:1.4rem;margin-top:2.2rem}a{color:#086578}code{font-size:.88em;overflow-wrap:anywhere}
pre{padding:1rem;background:#eaf3f3;white-space:pre-wrap}li{margin:.55rem 0}
@media print{body{margin:0;max-width:none;font-size:11pt}h2{break-after:avoid}pre{break-inside:avoid}}"""
    output = '<!doctype html><html lang="en"><meta charset="utf-8"><title>Maintain the kids UI revamp</title>'
    output += "<style>" + style + "</style><main>" + "\n".join(blocks) + "</main></html>"
    target = SOURCE.with_suffix(".html")
    target.write_text(output, encoding="utf-8")
    print(target)


if __name__ == "__main__":
    export()
