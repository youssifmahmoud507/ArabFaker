# Security policy

## Supported versions

ArabFaker.NET is in preview. Only the latest published version receives fixes.

## Reporting a vulnerability

Please do **not** open a public issue for a security problem.

Report it privately with GitHub's **Security > Report a vulnerability** form on this repository
(private vulnerability reporting). Include the version, what you did, and what happened.

You can expect an answer within a few days. Confirmed problems are fixed in a new release and
described in `CHANGELOG.md` once a fix is available.

## What counts

ArabFaker.NET generates fake data and is not meant for security purposes. Its random generator
(Xoshiro256**) is deterministic on purpose and must never be used for passwords, tokens or keys.
Generated values such as national ID numbers are random digits with no meaning and are test data only.

Reports we care about include: code execution or injection through exported data (for example SQL
or CSV injection through generated text), denial of service from small inputs, and vulnerable
dependencies in the published package.
