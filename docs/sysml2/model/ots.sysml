package OtsDependencies {
    doc /* OTS dependencies used by BuildMark. */

    part def YamlDotNet {
        doc /* OTS: YAML parsing library used to read/write .buildmark.yaml configuration. */

        comment designRef /* Design: docs/design/ots/yaml-dot-net.md */
        // Note: no verificationRef/reqRef - docs/verification/ots/ and docs/reqstream/ots/
        // do not contain a yaml-dot-net entry (discovered pre-existing gap, not fabricated).
    }
}
