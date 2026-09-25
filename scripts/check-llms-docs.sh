#!/usr/bin/env bash
#
# Drift guard for the llms.txt / llms-full.txt pairs (see CLAUDE.md, "LLM Documentation").
# Runs on CI and locally: `bash scripts/check-llms-docs.sh`. Prints every finding and exits
# non-zero if there was at least one.
#
# Checks:
#   1. Every packable src/ project carries llms.txt and llms-full.txt, or names the package that
#      documents it in <LlmsDocumentedBy> and is mentioned in that package's llms-full.txt.
#   2. The H1 of both files is the PackageId, and the blockquote summary is identical in both.
#   3. Every repository link in the pair is on the main branch, points to a path that exists, and
#      never to PublicAPI.Unshipped.txt.
#   4. Every public type and member named in the PublicAPI files is mentioned in llms-full.txt.
#   5. The root llms.txt links every package's llms-full.txt.
#
# Line endings are deliberately not checked: the repository stores LF (`* text=auto`) and the
# working copy follows the platform, so CRLF is an editor concern handled by .editorconfig.

set -u

cd "$(dirname "$0")/.."

failures=0

fail() {
  printf 'llms-docs: %s\n' "$1" >&2
  failures=$((failures + 1))
}

raw_prefix='https://codeberg.org/DerPeit/Josupeit.Practices/raw/branch/main/'
tree_prefix='https://codeberg.org/DerPeit/Josupeit.Practices/src/branch/main/'

# Pulls a property value out of a csproj; empty when the property is absent.
csproj_property() {
  tr -d '\r' < "$1" | sed -n "s|.*<$2>\(.*\)</$2>.*|\1|p" | head -1
}

# The blockquote is the first line starting with '> ' up to the next blank line.
quote() {
  tr -d '\r' < "$1" | awk '/^> /{q=1} q&&/^$/{exit} q{print}'
}

# Prints the simple name of every type and member declared in a PublicAPI file, one per line,
# deduplicated. Operators and removed entries carry no name of their own.
api_names() {
  tr -d '\r' < "$1" | awk '
    /^#nullable/ || /^\*REMOVED\*/ || / operator / || /^$/ { next }
    # Delegate types get Invoke/BeginInvoke/EndInvoke from the compiler; no document names them.
    /\.(Invoke|BeginInvoke|EndInvoke)\(/ { next }
    {
      sub(/^(abstract|static|virtual|override|const|readonly) /, "")
      sub(/ -> .*$/, "")
      sub(/ = .*$/, "")     # a constant is listed with its value
      sub(/\(.*$/, "")
      sub(/\.(get|set|add|remove)$/, "")
      while (gsub(/<[^<>]*>/, "")) { }
      n = split($0, parts, ".")
      name = parts[n]
      sub(/^~/, "", name)   # a finalizer is listed as ~Type
      print name
    }' | sort -u
}

for csproj in src/*/*.csproj; do
  dir="$(dirname "$csproj")"
  name="$(basename "$dir")"

  [ "$(csproj_property "$csproj" IsPackable)" = "false" ] && continue

  # ---- 1. Existence ----------------------------------------------------------------------------
  documented_by="$(csproj_property "$csproj" LlmsDocumentedBy)"

  if [ -n "$documented_by" ]; then
    host="src/$documented_by/llms-full.txt"

    if [ -f "$host" ]; then
      grep -q -F "$name" "$host" || fail "$name: not mentioned in $host"
    else
      fail "$name: LlmsDocumentedBy names $documented_by, but $host does not exist"
    fi

    continue
  fi

  index="$dir/llms.txt"
  full="$dir/llms-full.txt"
  missing=0

  for file in "$index" "$full"; do
    [ -s "$file" ] || { fail "$name: $file is missing or empty"; missing=1; }
  done

  [ "$missing" -eq 0 ] || continue

  # ---- 2. H1 and blockquote --------------------------------------------------------------------
  package_id="$(csproj_property "$csproj" PackageId)"
  [ -n "$package_id" ] || package_id="$name"

  for file in "$index" "$full"; do
    h1="$(head -1 "$file" | tr -d '\r')"
    [ "$h1" = "# $package_id" ] || fail "$file: first line is '$h1', expected '# $package_id'"
  done

  [ -n "$(quote "$full")" ] || fail "$full: no blockquote summary"
  [ "$(quote "$index")" = "$(quote "$full")" ] || fail "$name: blockquote summary differs between llms.txt and llms-full.txt"

  # ---- 3. Links --------------------------------------------------------------------------------
  for file in "$index" "$full"; do
    while read -r url; do
      case "$url" in
        *PublicAPI.Unshipped*) fail "$file: links $url (never link the Unshipped file)" ;;
        "$raw_prefix"*) [ -e "${url#"$raw_prefix"}" ] || fail "$file: $url does not exist in the tree" ;;
        "$tree_prefix"*) [ -e "${url#"$tree_prefix"}" ] || fail "$file: $url does not exist in the tree" ;;
        *) fail "$file: $url is not on the main branch" ;;
      esac
    done < <(grep -o 'https://codeberg.org/DerPeit/Josupeit.Practices/[^ )]*' "$file" | tr -d '\r' | sed 's/[.,;:]$//' | sort -u)
  done

  # ---- 4. API coverage -------------------------------------------------------------------------
  for api in "$dir"/PublicAPI.Shipped*.txt "$dir"/PublicAPI.Unshipped*.txt; do
    [ -f "$api" ] || continue

    while read -r member; do
      fail "$full: does not mention '$member' ($(basename "$api"))"
    done < <(comm -23 <(api_names "$api") <(grep -o -F -f <(api_names "$api") "$full" | sort -u))
  done
done

# ---- 5. Root index -----------------------------------------------------------------------------
if [ -s llms.txt ]; then
  for full in src/*/llms-full.txt; do
    grep -q -F "$raw_prefix$full" llms.txt || fail "llms.txt: does not link $full"
  done
else
  fail "llms.txt: root index is missing"
fi

if [ "$failures" -gt 0 ]; then
  printf 'llms-docs: %d finding(s)\n' "$failures" >&2
  exit 1
fi

echo "llms-docs: all checks passed"
