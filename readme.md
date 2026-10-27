---
id: a5edbdd6-fca9-4c79-97db-797d5221c155
title: readme
tags: []
pinned: false
created: 2026-10-27T03:03:23.229262800+00:00
modified: 2026-10-27T03:25:16.891819600+00:00
---
# readme

# ExtHide - para esconder arquivos em lote por meio da extensão

Meu primeiro projeto depois de 12 anos longe de linhas de código, com a ajuda (bem forte) do Claude.

## A ideia

Retirar a visibilidade de todos os arquivos de uma pasta no Windows que contanham a mesma extensão, usando a função de arquivos ocultos do Windows.

## De onde surgiu?

Quando comecei a importar minhas fotos para o Darktable, vi que o programa criava arquivos de backup na mesma pasta. Quando comecei a importar fotos demais, a bagunça ficou insustentável, e tive a ideia de criar esse executável em C# para esconder todos os arquivos com a extensão usada pelo programa, em lote.

## Como usar

1. Baixe e salve o programa em qualquer pasta. Ele não precisa estar onde os arquivos estão para funcionar;
2. Selecione o diretório que você pretende trabalhar;
3. Escolha as extensões de arquivo disponíveis;
4. Divirta-se.

## Requerimentos

1. Windows (é uma aplicação WinForms);
2. .NET 8 SDk

## Outras observações

Como informado mais ali pra cima, a maior parte do código, mesmo que simples, foi escrita com o Claude. Faz tempo que não chego nem perto de uma linha de código, então começar do zero ia ser, além de nada prático, demorado. Na minha visão, a IA serve justamente para ajudar nesses momentos. Além disso, é um executável simples, pra facilitar minha vida, sem qualquer outra pretensão além dessa.

Justamente por isso, o uso é livre, desde que 1. faça as referências necessárias, e 2. deixe claro que parte desse código usa IA.