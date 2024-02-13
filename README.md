# libz-unpack

This is a simple tool to unfold assemblies that have been merged using [LibZ](https://github.com/MiloszKrajewski/LibZ).

Usage:

```shell
libz-unpack.exe <input> <output>
```

Where:

- `<input>` (required) is the path to an exe or dll or a directory containing them
- `<output>` (required) is the path to the output directory
- `-r` (optional) recursive mode, which searches every unpacked assembly for more merged assemblies
