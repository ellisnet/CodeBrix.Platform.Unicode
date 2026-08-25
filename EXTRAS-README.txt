================================================================================
EXTRAS-README: CodeBrix.Platform.Unicode
Samples, tools and other content in this repository that is not part of a NuGet
package
================================================================================

This repository contains no samples, demo applications, tools or optional
test-data downloads. Everything under src/ is packaged content; the only
non-package content is the two test projects under tests/.


TEST PROJECTS
=============

    tests/CodeBrix.Platform.Unicode.Tests/
    tests/CodeBrix.Platform.UnicodeMacOs.Tests/

One suite per package, xUnit v3 with SilverAssertions, neither of them packable.
Run both from the repository root with:

    dotnet test CodeBrix.Platform.Unicode.slnx

They are asset and MSBuild-contract tests rather than behavioural tests: they
never load ICU or call a native entry point. Each test project copies its
package's payload - the native binaries, icudt.dat and both .targets files -
into a TestAssets/ folder in its own build output, mirroring the in-package
folder shape, then asserts that the files are present, that the binaries carry
the right file-format magic bytes and plausible sizes, that the .targets
declare the expected target names, conditions and items, and that the carrier
assembly targets .NET 10 and exports no public types.

Because a xUnit v3 test project is an executable and is not a
CodeBrix.Platform head, each suite is also a live example of the packages'
copy-beside delivery route: building it drops icudt.dat next to the test
binary. See AGENT-README.txt for what that route is and when it fires, and
MAINTAINER-README.txt for the per-test detail and the known coverage gap.


TEST DATA
=========

None beyond the package payload described above. There is nothing to download
and no environment variable to set.
