import sys

path = 'backend/src/HorseRacing.Infrastructure/Persistence/DemoDataSeeder.cs'
with open(path, 'r', encoding='utf-8') as f:
    content = f.read()

marker_start = '// 3. Seed Tournament 1: "Giải Đua Ngựa Mùa Xuân 2026" (FINISHED)'
marker_end = '// Always recalculate and force update Odds for Summer Race on every startup'

if marker_start in content and marker_end in content:
    content = content.replace(marker_start, '{\n            ' + marker_start)
    content = content.replace(marker_end, '}\n            ' + marker_end)
    with open(path, 'w', encoding='utf-8') as f:
        f.write(content)
    print('Success')
else:
    print('Markers not found')
