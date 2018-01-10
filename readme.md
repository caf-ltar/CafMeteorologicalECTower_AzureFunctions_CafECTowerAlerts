### Purpose

Azure Function app that monitors Flux data tables retrieved from Campbell Scientific datalogger and copied to Azure Blob storage.  The validation is very basic, mostly a few range checks and null checks.  If a check fails then a Tweet is posted by @CafECTowerBot indicating the file name (and from that the date and time) and the failed check(s).