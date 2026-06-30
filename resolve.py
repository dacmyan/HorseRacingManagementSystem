import sys
def resolve_file(filepath):
    with open(filepath, 'r', encoding='utf-8') as f:
        content = f.read()
    lines = content.splitlines(keepends=True)
    out_lines = []
    state = 0
    head_lines = []
    theirs_lines = []
    conflict_count = 0
    for line in lines:
        if line.startswith('<<<<<<<'):
            state = 1
            conflict_count += 1
            head_lines = []
            theirs_lines = []
            continue
        elif line.startswith('======='):
            state = 2
            continue
        elif line.startswith('>>>>>>>'):
            state = 0
            if conflict_count == 1:
                out_lines.append('                new() { Username = "owner",     Email = "owner@gmail.com",     FullName = "Nguyễn Văn Hùng", RoleId = 2 },\n')
                out_lines.append('                new() { Username = "owner2",    Email = "owner2@gmail.com",    FullName = "Trần Thị Mai",     RoleId = 2 },\n')
                out_lines.append('                new() { Username = "owner3",    Email = "owner3@gmail.com",    FullName = "Lê Minh Tuấn",     RoleId = 2 },\n')
                out_lines.append('                new() { Username = "jockey",    Email = "jockey@gmail.com",    FullName = "Jockey Nguyễn",    RoleId = 3 },\n')
                out_lines.append('                new() { Username = "referee",   Email = "referee@gmail.com",   FullName = "Trọng tài Nam",    RoleId = 4 },\n')
                out_lines.append('                new() { Username = "spectator", Email = "spectator@gmail.com", FullName = "Khán giả Bình",    RoleId = 5 },\n')
                out_lines.append('                new() { Username = "spectator2", Email = "spectator2@gmail.com", FullName = "Khán giả Hoàng",   RoleId = 5 },\n')
                out_lines.append('                new() { Username = "spectator3", Email = "spectator3@gmail.com", FullName = "Khán giả Dung",    RoleId = 5 }\n')
            else:
                out_lines.extend(head_lines)
                out_lines.extend(theirs_lines)
            continue
        if state == 0:
            out_lines.append(line)
        elif state == 1:
            head_lines.append(line)
        elif state == 2:
            theirs_lines.append(line)
    with open(filepath, 'w', encoding='utf-8') as f:
        f.writelines(out_lines)
    print(f'Resolved {conflict_count} conflicts')
resolve_file(r'backend\src\HorseRacing.Infrastructure\Persistence\DemoDataSeeder.cs')
