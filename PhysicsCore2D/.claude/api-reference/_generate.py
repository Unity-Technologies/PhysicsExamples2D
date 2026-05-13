"""
Generate PhysicsCore2D API reference markdown from Unity's IntelliSense XML.

Source XML: <UnityEditor>/Data/Managed/UnityEngine/UnityEngine.PhysicsCore2DModule.xml
Output: ./*.md (one file per type cluster) + INDEX.md

Re-runnable: deterministic output, no network calls. Online ScriptReference
augmentation is layered in by a separate step.
"""

from __future__ import annotations

import re
import sys
import xml.etree.ElementTree as ET
from collections import defaultdict
from dataclasses import dataclass, field
from pathlib import Path
from typing import Iterable

XML_PATH = Path(
    r"C:\Program Files\Unity\Hub\Editor\6000.5.0b7\Editor\Data\Managed\UnityEngine\UnityEngine.PhysicsCore2DModule.xml"
)
OUT_DIR = Path(__file__).resolve().parent
NAMESPACE = "Unity.U2D.Physics"
DOCS_BASE = "https://docs.unity3d.com/6000.5/Documentation/ScriptReference"

# Map top-level type -> cluster file name (without .md).
# Types not listed default to "misc".
CLUSTERS: dict[str, str] = {
    # world
    "PhysicsWorld": "world",
    "PhysicsWorldDefinition": "world",
    "PhysicsCoreSettings2D": "world",
    "PhysicsConstants": "world",
    # bodies
    "PhysicsBody": "bodies",
    "PhysicsBodyDefinition": "bodies",
    # shapes
    "PhysicsShape": "shapes",
    "PhysicsShapeDefinition": "shapes",
    # chains
    "PhysicsChain": "chains",
    "PhysicsChainDefinition": "chains",
    # geometry
    "CapsuleGeometry": "geometry",
    "CircleGeometry": "geometry",
    "PolygonGeometry": "geometry",
    "SegmentGeometry": "geometry",
    "ChainGeometry": "geometry",
    "ChainSegmentGeometry": "geometry",
    "PhysicsAABB": "geometry",
    "PhysicsPlane": "geometry",
    # joints
    "PhysicsJoint": "joints",
    "PhysicsDistanceJoint": "joints",
    "PhysicsDistanceJointDefinition": "joints",
    "PhysicsFixedJoint": "joints",
    "PhysicsFixedJointDefinition": "joints",
    "PhysicsHingeJoint": "joints",
    "PhysicsHingeJointDefinition": "joints",
    "PhysicsIgnoreJoint": "joints",
    "PhysicsIgnoreJointDefinition": "joints",
    "PhysicsRelativeJoint": "joints",
    "PhysicsRelativeJointDefinition": "joints",
    "PhysicsSliderJoint": "joints",
    "PhysicsSliderJointDefinition": "joints",
    "PhysicsWheelJoint": "joints",
    "PhysicsWheelJointDefinition": "joints",
    # queries
    "PhysicsQuery": "queries",
    # events
    "PhysicsEvents": "events",
    "PhysicsCallbacks": "events",
    # composer / destructor
    "PhysicsComposer": "composer",
    "PhysicsDestructor": "destructor",
    # layers
    "PhysicsLayers": "layers",
    "PhysicsMask": "layers",
    # helpers
    "PhysicsTransform": "helpers",
    "PhysicsRotate": "helpers",
    "PhysicsUserData": "helpers",
    # math
    "PhysicsMath": "math",
}

CLUSTER_TITLES: dict[str, str] = {
    "world": "World & Simulation",
    "bodies": "Bodies",
    "shapes": "Shapes",
    "chains": "Chains",
    "geometry": "Geometry Primitives",
    "joints": "Joints",
    "queries": "Queries",
    "events": "Events & Callbacks",
    "composer": "Composer",
    "destructor": "Destructor",
    "layers": "Layers & Masks",
    "helpers": "Helpers",
    "math": "Math Utilities",
    "misc": "Miscellaneous",
}

# Skill -> list of cluster files that skill should consult.
SKILL_MAP: dict[str, list[str]] = {
    "unity-physicscore2d": ["world", "bodies", "shapes", "joints"],
    "unity-physicscore2d-batching": ["bodies", "queries"],
    "unity-physicscore2d-collision": ["shapes", "events", "world"],
    "unity-physicscore2d-components": ["bodies", "shapes", "world"],
    "unity-physicscore2d-composer": ["composer", "shapes"],
    "unity-physicscore2d-debug": ["world"],
    "unity-physicscore2d-destruction": ["destructor", "shapes"],
    "unity-physicscore2d-destructor": ["destructor"],
    "unity-physicscore2d-events": ["events", "world"],
    "unity-physicscore2d-factories": ["bodies", "shapes", "joints", "chains"],
    "unity-physicscore2d-filtering": ["layers", "shapes", "queries"],
    "unity-physicscore2d-forces": ["bodies", "shapes"],
    "unity-physicscore2d-geometry": ["geometry"],
    "unity-physicscore2d-helpers": ["helpers", "layers", "math"],
    "unity-physicscore2d-joints": ["joints"],
    "unity-physicscore2d-layers": ["layers", "shapes"],
    "unity-physicscore2d-materials": ["shapes"],
    "unity-physicscore2d-math": ["math", "helpers"],
    "unity-physicscore2d-performance": ["world", "bodies", "queries"],
    "unity-physicscore2d-queries": ["queries", "geometry"],
    "unity-physicscore2d-settings": ["world"],
    "unity-physicscore2d-shapes-advanced": ["shapes", "chains", "geometry"],
    "unity-physicscore2d-stacking": ["bodies", "shapes", "joints"],
}

# ---------------------------------------------------------------------------
# Parsing
# ---------------------------------------------------------------------------


@dataclass
class Member:
    kind: str  # T, P, F, M, E
    full_name: str  # e.g. PhysicsBody.SetAwake or PhysicsBody/BatchForce
    type_path: str  # owning type chain, e.g. PhysicsBody or PhysicsBody.BatchForce
    member_name: str  # final segment, with method signature for M
    raw_member_name: str  # final segment without signature
    summary: str = ""
    params: list[tuple[str, str]] = field(default_factory=list)
    returns: str = ""

    @property
    def is_type(self) -> bool:
        return self.kind == "T"

    @property
    def is_nested(self) -> bool:
        return "." in self.type_path or "/" in self.type_path


_PREFIX_RE = re.compile(r"\bUnity\.U2D\.Physics\.")


def _flatten_text(elem: ET.Element | None) -> str:
    if elem is None:
        return ""
    # Concatenate all text inside, stripping <para> wrappers.
    parts: list[str] = []
    for piece in elem.itertext():
        parts.append(piece)
    text = " ".join(parts).strip()
    # Collapse internal whitespace.
    text = re.sub(r"\s+", " ", text)
    # Strip the noisy fully-qualified namespace prefix in cross-references.
    text = _PREFIX_RE.sub("", text)
    return text


def parse_member(elem: ET.Element) -> Member | None:
    name_attr = elem.attrib.get("name", "")
    if not name_attr or ":" not in name_attr:
        return None
    kind, qualified = name_attr.split(":", 1)
    if kind not in {"T", "P", "F", "M", "E"}:
        return None
    if not qualified.startswith(NAMESPACE + "."):
        return None
    rest = qualified[len(NAMESPACE) + 1 :]

    # Split off method signature parens before path parsing.
    sig = ""
    if "(" in rest:
        idx = rest.index("(")
        sig = rest[idx:]
        rest = rest[:idx]

    # Path uses '.' for namespace/type separation and '/' for nested types
    # in M:/P:/F:/E:; T: just uses '.' (nested types have '.' too).
    # Normalize '/' -> '.' for type path, then peel off the final segment for
    # P/F/M/E.
    rest_norm = rest.replace("/", ".")

    if kind == "T":
        type_path = rest_norm
        raw_name = rest_norm.rsplit(".", 1)[-1]
        member_name = raw_name
        full_name = rest_norm
    else:
        # final segment is the member name
        if "." not in rest_norm:
            return None
        type_path, raw_name = rest_norm.rsplit(".", 1)
        member_name = raw_name + sig
        full_name = rest_norm + sig

    summary = _flatten_text(elem.find("summary"))
    params = [
        (p.attrib.get("name", ""), _flatten_text(p))
        for p in elem.findall("param")
    ]
    returns = _flatten_text(elem.find("returns"))

    return Member(
        kind=kind,
        full_name=full_name,
        type_path=type_path,
        member_name=member_name,
        raw_member_name=raw_name,
        summary=summary,
        params=params,
        returns=returns,
    )


def parse_xml(path: Path) -> list[Member]:
    tree = ET.parse(path)
    root = tree.getroot()
    members: list[Member] = []
    for elem in root.findall(".//member"):
        m = parse_member(elem)
        if m is not None:
            members.append(m)
    return members


# ---------------------------------------------------------------------------
# Type-shape decoding
# ---------------------------------------------------------------------------

PRIMITIVE_MAP = {
    "System.Single": "float",
    "System.Double": "double",
    "System.Int32": "int",
    "System.UInt32": "uint",
    "System.Int64": "long",
    "System.UInt64": "ulong",
    "System.Int16": "short",
    "System.UInt16": "ushort",
    "System.Byte": "byte",
    "System.SByte": "sbyte",
    "System.Boolean": "bool",
    "System.String": "string",
    "System.Object": "object",
    "System.Void": "void",
    "System.IntPtr": "IntPtr",
    "UnityEngine.Vector2": "Vector2",
    "UnityEngine.Vector3": "Vector3",
    "UnityEngine.Vector4": "Vector4",
    "UnityEngine.Quaternion": "Quaternion",
    "UnityEngine.Matrix4x4": "Matrix4x4",
    "UnityEngine.Color": "Color",
    "UnityEngine.Color32": "Color32",
    "UnityEngine.Bounds": "Bounds",
    "UnityEngine.Rect": "Rect",
}


def short_type(t: str) -> str:
    """Render a fully-qualified IL type name in short C# form."""
    t = t.strip()

    # Manually shorten `N<...> generic markers — the IL form e.g.
    # ReadOnlySpan`1<Unity.U2D.Physics.PhysicsJoint>.
    def shorten_generics(s: str) -> str:
        out_chars: list[str] = []
        i = 0
        while i < len(s):
            ch = s[i]
            if ch == "`" and i + 1 < len(s) and s[i + 1].isdigit():
                # Skip `N
                j = i + 1
                while j < len(s) and s[j].isdigit():
                    j += 1
                # If followed by '<', recurse on contents.
                if j < len(s) and s[j] == "<":
                    depth = 0
                    k = j
                    while k < len(s):
                        if s[k] == "<":
                            depth += 1
                        elif s[k] == ">":
                            depth -= 1
                            if depth == 0:
                                break
                        k += 1
                    inner = s[j + 1 : k]
                    # Split on top-level commas.
                    args: list[str] = []
                    d2 = 0
                    buf = ""
                    for ch2 in inner:
                        if ch2 in "<({[":
                            d2 += 1
                            buf += ch2
                        elif ch2 in ">)}]":
                            d2 -= 1
                            buf += ch2
                        elif ch2 == "," and d2 == 0:
                            args.append(buf)
                            buf = ""
                        else:
                            buf += ch2
                    if buf:
                        args.append(buf)
                    out_chars.append("<" + ", ".join(short_type(a) for a in args) + ">")
                    i = k + 1
                    continue
                else:
                    i = j
                    continue
            out_chars.append(ch)
            i += 1
        return "".join(out_chars)

    t = shorten_generics(t)

    # Suffixes (arrays, by-ref, pointer)
    suffixes = ""
    while t.endswith("[]") or t.endswith("@") or t.endswith("*") or t.endswith("&"):
        if t.endswith("[]"):
            suffixes = "[]" + suffixes
            t = t[:-2]
        else:
            t = t[:-1]  # @ and & are by-ref markers we drop visually
    # Strip nested-type marker
    t = t.replace("/", ".")
    if t in PRIMITIVE_MAP:
        return PRIMITIVE_MAP[t] + suffixes
    # Strip Unity.U2D.Physics. namespace prefix
    if t.startswith(NAMESPACE + "."):
        t = t[len(NAMESPACE) + 1 :]
    if t.startswith("UnityEngine."):
        t = t[len("UnityEngine.") :]
    if t.startswith("System."):
        t = t[len("System.") :]
    # Also strip any lingering generic-arity backticks that didn't have <>.
    t = re.sub(r"`\d+", "", t)
    return t + suffixes


def parse_signature(sig: str) -> list[str]:
    """Parse a method signature like '(Unity.U2D.Physics.X,System.Single)' into
    ['X', 'float']. Handles nested generics and arrays."""
    sig = sig.strip()
    if not sig.startswith("(") or not sig.endswith(")"):
        return []
    inner = sig[1:-1]
    if not inner:
        return []
    # Split on commas at depth 0.
    parts: list[str] = []
    depth = 0
    buf = ""
    for ch in inner:
        if ch in "({[<":
            depth += 1
            buf += ch
        elif ch in ")}]>":
            depth -= 1
            buf += ch
        elif ch == "," and depth == 0:
            parts.append(buf)
            buf = ""
        else:
            buf += ch
    if buf:
        parts.append(buf)
    return [short_type(p) for p in parts]


# ---------------------------------------------------------------------------
# Grouping
# ---------------------------------------------------------------------------


@dataclass
class TypeNode:
    name: str  # e.g. PhysicsBody.BatchForce
    short_name: str  # final segment, e.g. BatchForce
    summary: str = ""
    properties: list[Member] = field(default_factory=list)
    fields: list[Member] = field(default_factory=list)
    methods: list[Member] = field(default_factory=list)
    events: list[Member] = field(default_factory=list)
    nested: list["TypeNode"] = field(default_factory=list)

    @property
    def is_nested(self) -> bool:
        return "." in self.name


def build_tree(members: list[Member]) -> dict[str, TypeNode]:
    """Returns top-level types keyed by name. Nested types attached under
    `.nested`."""
    nodes: dict[str, TypeNode] = {}

    # First pass: create a node per T:.
    for m in members:
        if m.is_type:
            short = m.full_name.rsplit(".", 1)[-1]
            nodes[m.full_name] = TypeNode(
                name=m.full_name, short_name=short, summary=m.summary
            )

    # Second pass: attach members to their owning type.
    for m in members:
        if m.is_type:
            continue
        owner = nodes.get(m.type_path)
        if owner is None:
            # Member of an unknown / undocumented type — skip.
            continue
        if m.kind == "P":
            owner.properties.append(m)
        elif m.kind == "F":
            owner.fields.append(m)
        elif m.kind == "M":
            owner.methods.append(m)
        elif m.kind == "E":
            owner.events.append(m)

    # Third pass: nest types under their parents.
    top: dict[str, TypeNode] = {}
    for full_name, node in nodes.items():
        if "." in full_name:
            parent_name = full_name.rsplit(".", 1)[0]
            parent = nodes.get(parent_name)
            if parent is not None:
                parent.nested.append(node)
                continue
        top[full_name] = node

    # Sort everything for determinism.
    for node in nodes.values():
        node.properties.sort(key=lambda x: x.raw_member_name.lower())
        node.fields.sort(key=lambda x: x.raw_member_name.lower())
        node.methods.sort(key=lambda x: x.raw_member_name.lower())
        node.events.sort(key=lambda x: x.raw_member_name.lower())
        node.nested.sort(key=lambda x: x.short_name.lower())

    return top


# ---------------------------------------------------------------------------
# Rendering
# ---------------------------------------------------------------------------


def format_method_heading(m: Member) -> str:
    name = m.raw_member_name
    sig = m.member_name[len(name) :]
    params = parse_signature(sig)
    # Pretty operator names
    op_map = {
        "op_Equality": "operator ==",
        "op_Inequality": "operator !=",
        "op_LessThan": "operator <",
        "op_GreaterThan": "operator >",
        "op_LessThanOrEqual": "operator <=",
        "op_GreaterThanOrEqual": "operator >=",
        "op_Addition": "operator +",
        "op_Subtraction": "operator -",
        "op_Multiply": "operator *",
        "op_Division": "operator /",
        "op_Modulus": "operator %",
        "op_BitwiseAnd": "operator &",
        "op_BitwiseOr": "operator |",
        "op_ExclusiveOr": "operator ^",
        "op_OnesComplement": "operator ~",
        "op_LeftShift": "operator <<",
        "op_RightShift": "operator >>",
        "op_UnaryNegation": "operator -",
        "op_UnaryPlus": "operator +",
        "op_LogicalNot": "operator !",
        "op_Implicit": "operator implicit",
        "op_Explicit": "operator explicit",
        "#ctor": "constructor",
    }
    pretty_name = op_map.get(name, name)
    if pretty_name == "constructor":
        return f"`new({', '.join(params)})`"
    if params:
        return f"`{pretty_name}({', '.join(params)})`"
    return f"`{pretty_name}()`"


def render_member_table(title: str, members: list[Member], section_level: int) -> list[str]:
    if not members:
        return []
    h = "#" * section_level
    out = [f"{h} {title}", "", "| Name | Summary |", "|------|---------|"]
    for m in members:
        summary = m.summary.replace("|", "\\|") or "—"
        out.append(f"| `{m.raw_member_name}` | {summary} |")
    out.append("")
    return out


def render_methods(members: list[Member], section_level: int) -> list[str]:
    if not members:
        return []
    section_h = "#" * section_level
    item_h = "#" * (section_level + 1)
    out = [f"{section_h} Methods", ""]
    by_name: dict[str, list[Member]] = defaultdict(list)
    for m in members:
        by_name[m.raw_member_name].append(m)
    for name in sorted(by_name.keys(), key=str.lower):
        for m in by_name[name]:
            heading = format_method_heading(m)
            out.append(f"{item_h} {heading}")
            if m.summary:
                out.append("")
                out.append(m.summary)
            if m.params:
                out.append("")
                out.append("**Params:**")
                for pname, pdesc in m.params:
                    out.append(f"- `{pname}` — {pdesc or '—'}")
            if m.returns:
                out.append("")
                out.append(f"**Returns:** {m.returns}")
            out.append("")
    return out


def render_type(node: TypeNode, level: int = 2) -> list[str]:
    h = "#" * level
    section_level = level + 1
    section_h = "#" * section_level
    out: list[str] = []
    short = node.short_name
    full = node.name
    out.append(f"{h} {short}")
    out.append("")
    if node.summary:
        out.append(f"> {node.summary}")
        out.append("")
    out.append(f"**Full name:** `{NAMESPACE}.{full}`  ")
    if "." not in full:
        docs_url = f"{DOCS_BASE}/{NAMESPACE}.{full}.html"
        out.append(f"**Docs:** [{NAMESPACE}.{short}]({docs_url})")
    out.append("")

    out.extend(render_member_table("Fields", node.fields, section_level))
    out.extend(render_member_table("Properties", node.properties, section_level))
    out.extend(render_member_table("Events", node.events, section_level))
    out.extend(render_methods(node.methods, section_level))

    if node.nested:
        out.append(f"{section_h} Nested Types")
        out.append("")
        for n in node.nested:
            summary = n.summary or "—"
            out.append(f"- **{n.short_name}** — {summary}")
        out.append("")
        for n in node.nested:
            out.extend(render_type(n, level=level + 1))

    return out


def render_cluster(cluster: str, top_types: list[TypeNode]) -> str:
    title = CLUSTER_TITLES.get(cluster, cluster)
    out: list[str] = [
        f"# PhysicsCore2D — {title}",
        "",
        f"_Generated from Unity 6000.5.0b7 `UnityEngine.PhysicsCore2DModule.xml`._",
        "",
        f"Top-level types in this file: {', '.join('`' + t.short_name + '`' for t in top_types)}.",
        "",
    ]
    for t in top_types:
        out.extend(render_type(t, level=2))
    return "\n".join(out).rstrip() + "\n"


def render_index(top_nodes: dict[str, TypeNode], cluster_assignment: dict[str, str]) -> str:
    out: list[str] = [
        "# PhysicsCore2D API Reference — Index",
        "",
        "_Generated from Unity 6000.5.0b7 `UnityEngine.PhysicsCore2DModule.xml`._",
        "",
        "All `Unity.U2D.Physics` top-level types, grouped by cluster file.",
        "",
    ]

    # Group top-level types by cluster.
    by_cluster: dict[str, list[TypeNode]] = defaultdict(list)
    for name, node in top_nodes.items():
        cluster = cluster_assignment.get(name, "misc")
        by_cluster[cluster].append(node)

    for cluster in sorted(by_cluster.keys(), key=lambda c: CLUSTER_TITLES.get(c, c)):
        title = CLUSTER_TITLES.get(cluster, cluster)
        out.append(f"## [{title}]({cluster}.md)")
        out.append("")
        out.append("| Type | Summary |")
        out.append("|------|---------|")
        for node in sorted(by_cluster[cluster], key=lambda n: n.short_name.lower()):
            summary = (node.summary or "—").replace("|", "\\|")
            out.append(f"| [`{node.short_name}`]({cluster}.md#{node.short_name.lower()}) | {summary} |")
        out.append("")

    out.append("## Skill → Reference Files")
    out.append("")
    out.append("Each `unity-physicscore2d-*` skill should consult the listed reference files when its content needs updating.")
    out.append("")
    out.append("| Skill | Reference files |")
    out.append("|-------|-----------------|")
    for skill in sorted(SKILL_MAP.keys()):
        files = ", ".join(f"[{c}]({c}.md)" for c in SKILL_MAP[skill])
        out.append(f"| `{skill}` | {files} |")
    out.append("")

    return "\n".join(out).rstrip() + "\n"


# ---------------------------------------------------------------------------
# Skill writer
# ---------------------------------------------------------------------------

SKILLS_DIR = (OUT_DIR.parent / "skills").resolve()


def render_skill(cluster: str, nodes: list[TypeNode], cluster_md_body: str) -> str:
    """Compose a SKILL.md file inlining the cluster markdown body."""
    title = CLUSTER_TITLES.get(cluster, cluster)
    type_names = [n.short_name for n in nodes]
    skill_name = f"unity-physicscore2d-{cluster}-api"
    type_list = ", ".join(type_names)
    description = (
        f"Authoritative Unity 6000.5 PhysicsCore2D API reference for {title}. "
        f"Lists every type, property, field, method (with signatures, params, returns) for: "
        f"{type_list}. Use whenever working with these types in code."
    )

    # Strip the original H1 line from the cluster markdown — the SKILL provides
    # its own. The first line is always `# PhysicsCore2D — <Title>` followed by
    # a blank line.
    body_lines = cluster_md_body.splitlines()
    # Remove the first H1 + the blank line that follows.
    if body_lines and body_lines[0].startswith("# "):
        body_lines = body_lines[1:]
        if body_lines and body_lines[0] == "":
            body_lines = body_lines[1:]
    stripped_body = "\n".join(body_lines).rstrip()

    out: list[str] = [
        "---",
        f"name: {skill_name}",
        f"description: {description}",
        "---",
        "",
        f"# Unity PhysicsCore2D API — {title}",
        "",
        "This skill is the auto-generated API surface for the listed types. "
        "It pre-dates Claude's training data on Unity 6000.5, so it should be "
        "treated as the source of truth for member names, signatures, and "
        "documentation strings.",
        "",
        stripped_body,
        "",
        "---",
        "",
        "_Generated by `.claude/api-reference/_generate.py` from Unity 6000.5.0b7 "
        "`UnityEngine.PhysicsCore2DModule.xml`. Do not hand-edit; re-run the "
        "generator._",
        "",
    ]
    return "\n".join(out)


def write_skill_files(by_cluster: dict[str, list[TypeNode]]) -> None:
    SKILLS_DIR.mkdir(parents=True, exist_ok=True)
    for cluster, nodes in sorted(by_cluster.items()):
        cluster_md_path = OUT_DIR / f"{cluster}.md"
        if not cluster_md_path.exists():
            print(f"  skipping {cluster}: source md not found", file=sys.stderr)
            continue
        cluster_md_body = cluster_md_path.read_text(encoding="utf-8")
        skill_dir = SKILLS_DIR / f"unity-physicscore2d-{cluster}-api"
        skill_dir.mkdir(parents=True, exist_ok=True)
        skill_path = skill_dir / "SKILL.md"
        content = render_skill(cluster, nodes, cluster_md_body)
        skill_path.write_text(content, encoding="utf-8")
        print(f"Wrote skill {skill_dir.name}/SKILL.md ({len(content)} bytes)")


# ---------------------------------------------------------------------------
# Main
# ---------------------------------------------------------------------------


def main() -> int:
    if not XML_PATH.exists():
        print(f"ERROR: XML not found: {XML_PATH}", file=sys.stderr)
        return 1

    members = parse_xml(XML_PATH)
    print(f"Parsed {len(members)} members from XML.")

    top_nodes = build_tree(members)
    print(f"Built {len(top_nodes)} top-level types.")

    # Group top-level into clusters.
    by_cluster: dict[str, list[TypeNode]] = defaultdict(list)
    cluster_assignment: dict[str, str] = {}
    for name, node in top_nodes.items():
        cluster = CLUSTERS.get(name, "misc")
        cluster_assignment[name] = cluster
        by_cluster[cluster].append(node)

    # Sort top-level within each cluster.
    for cluster in by_cluster:
        by_cluster[cluster].sort(key=lambda n: n.short_name.lower())

    # Write per-cluster files.
    for cluster, nodes in sorted(by_cluster.items()):
        path = OUT_DIR / f"{cluster}.md"
        content = render_cluster(cluster, nodes)
        path.write_text(content, encoding="utf-8")
        print(f"Wrote {path.name} ({len(nodes)} top-level types, {len(content)} bytes)")

    # Write INDEX.
    idx = render_index(top_nodes, cluster_assignment)
    (OUT_DIR / "INDEX.md").write_text(idx, encoding="utf-8")
    print(f"Wrote INDEX.md ({len(idx)} bytes)")

    # Write skill files (must run after cluster .md files exist on disk).
    write_skill_files(by_cluster)

    return 0


if __name__ == "__main__":
    sys.exit(main())
