#!/usr/bin/env python3
"""Write the shapes/ folder from the history of the exercism/csharp track repo.

A shape is a distinct xUnit v3 project file, ignoring the project name and the settings that do not
affect compiling. This script reads every exercise's .csproj in every commit since the track
switched to xUnit v3, groups them by shape, and writes one representative project file per shape to
<out_dir>/<n>/ExercismShape.csproj, with net9.0 replaced by net10.0 as the runner does. The Dockerfile
builds each one (bin/build-shapes.sh), so that the fast path can reuse its compiler arguments.

The grouping here is close to ProjectShape.Key in the runner. If the two ever disagree, the worst
case will be a duplicate template, or a project file with no matching template, which uses MSBuild.

usage: generate-shapes.py <out_dir> [path to the exercism/csharp repo, default ../csharp]"""
import subprocess, sys, os, shutil, xml.etree.ElementTree as ET
T = sys.argv[2] if len(sys.argv) > 2 else '../csharp'
out = sys.argv[1]
IGNORED = {'RestorePackagesWithLockFile', 'IsPackable', 'IsTestProject'}

def git(*a):
    return subprocess.run(['git', '-C', T, *a], capture_output=True, check=True, text=True).stdout

def key(text):
    root = ET.fromstring(text.replace('net9.0', 'net10.0'))
    props, items = {}, []
    for g in root:
        for e in g:
            if g.tag == 'PropertyGroup':
                if e.tag not in IGNORED: props[e.tag] = (e.text or '').strip()
            else:
                items.append((e.tag, tuple(sorted(e.attrib.items())) + tuple(sorted((m.tag, (m.text or '').strip()) for m in e))))
    return (tuple(sorted(props.items())), tuple(sorted(items)))

commits = git('log', '--format=%H', '--since=2025-02-24', '--', 'exercises').split() + ['HEAD']
shapes = {}
for c in commits:
    for path in git('ls-tree', '-r', '--name-only', c, 'exercises').split('\n'):
        if not path.endswith('.csproj'): continue
        text = git('show', f'{c}:{path}')
        if 'xunit.v3' not in text: continue
        k = key(text)
        shapes.setdefault(k, (text, set()))[1].add(path.split('/')[2])
shutil.rmtree(out, ignore_errors=True)
for i, (k, (text, exs)) in enumerate(sorted(shapes.items(), key=lambda kv: -len(kv[1][1]))):
    d = os.path.join(out, f'{i:02d}'); os.makedirs(d)
    open(os.path.join(d, 'ExercismShape.csproj'), 'w').write(text.replace('net9.0', 'net10.0'))
    print(i, len(exs), sorted(exs)[:3], dict(k[0]).get('TargetFramework'), [x for x in k[1] if x[0] == 'PackageReference'])
