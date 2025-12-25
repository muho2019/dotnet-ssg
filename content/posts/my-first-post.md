---
title: 'My First Blog Post'
date: '2023-10-27 12:10:01'
tags:
  - introduction
  - ssg
  - dotnet
---

Welcome to my new blog! This is my very first post, generated using my custom .NET Static Site Generator.

I've been working on this project to learn more about:

- **Static Site Generation:** The process of building HTML files in advance, which are then served directly to users. This offers great performance and security benefits.
- **Markdown for Content:** Writing content in a simple, readable format that can be easily converted to HTML.
- **.NET Development:** Leveraging the power and flexibility of .NET for various tasks, including text processing, file operations, and templating.
- **Scriban Templating:** An amazing templating engine that allows for dynamic content injection into static HTML layouts.

I'm excited to share my journey and insights here. Stay tuned for more updates!

## 코드 하이라이팅 예제

### 기본 코드 블럭

```csharp
Console.WriteLine("Hello, SSG!");
```

### 줄 번호가 있는 코드 블럭

```csharp
using System;

namespace MyApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Welcome to .NET SSG!");
        }
    }
}
```

### 특정 라인 강조

```javascript
function greet(name) {
  console.log('Hello, ' + name + '!');
}

const message = '하이라이팅된 라인입니다';

function calculate(a, b) {
  // 이 부분도 하이라이트됩니다
  const result = a + b;
  return result;
}

greet('World');
```

### Python 예제

```python
def fibonacci(n):
    if n <= 1:
        return n
    else:
        return fibonacci(n-1) + fibonacci(n-2)

print(fibonacci(10))
```
