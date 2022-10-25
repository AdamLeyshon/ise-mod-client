#!/bin/bash
protoc --csharp_out="./ise-core/packets/" -I ./proto $(find ./proto/ -iname "*.proto")
