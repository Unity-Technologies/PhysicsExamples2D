# Attribution

Third-party work this package builds on, and the terms it comes under.

Each entry below was checked against its source: the licence named is the one the source itself states.

## Code

**GMShaders-Radiance-Cascades**, by Yaazarai.

- Source: https://github.com/Yaazarai/GMShaders-Radiance-Cascades
- Licence: The Unlicense. The repository's licence file releases the work into the public domain.
- Ported from commit `355b2a4`.
- Used in: `Runtime/Shaders/Cascade.shader`.
- What was taken: the direction-first cascade layout, the merge between one cascade and the one above it, and the bilinear and
  forking fixes, ported from
  `RadianceCascades-Optimized/shaders/Shd_RadianceCascades_BilinearFix/Shd_RadianceCascades_BilinearFix.fsh`.

**ACES filmic tonemapping fit**, from Baking Lab, by MJP and David Neubelt, http://mynameismjp.wordpress.com/.

- Source: https://github.com/TheRealMJP/BakingLab/blob/master/BakingLab/ACES.hlsl
- Licence: MIT, stated both in the repository's licence file and in that file's own header.
- The fit itself was written by Stephen Hill (@self_shadow). That file's header records that he "deserves all credit for coming up
  with this fit and implementing it".
- Used in: `Runtime/Shaders/Composite.shader`, the `TonemapACES` function.
- What was taken: the two colour space matrices and the rational curve standing in for the ACES reference rendering and output
  transforms.

The MIT licence requires its copyright and permission notice to travel with the code, so it is reproduced here in full:

> Copyright (c) 2016 MJP
>
> Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation
> files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy,
> modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software
> is furnished to do so, subject to the following conditions:
>
> The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
>
> THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
> OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
> LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR
> IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

## Technique

**Radiance Cascades**, by Alexander Sannikov, who publishes as Raikiri on GitHub.

The lighting in this project implements the technique his paper describes. No code was taken from it.

- Paper source: https://github.com/Raikiri/RadianceCascadesPaper
- Licence: MIT. The paper is unpublished; the author notes it was never submitted to a journal.
