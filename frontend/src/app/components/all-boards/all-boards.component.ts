import { Component, OnInit } from '@angular/core';
import { Board } from '../../models/board';
import { BoardService } from '../../services/board.service';
import { MatSnackBar } from '@angular/material/snack-bar';
@Component({
  selector: 'app-all-boards',
  templateUrl: './all-boards.component.html',
  styleUrls: ['./all-boards.component.scss']
})
export class AllBoardsComponent implements OnInit {

  title: string;
  public static boards: Board[];

  constructor( private boardService: BoardService, private _snackBar: MatSnackBar ) { }

  ngOnInit() {
    this.title = "All Boards"
    this.getBoards();
  }

  async getBoards() {
    (await this.boardService.getBoards())
    .subscribe(data => {
      AllBoardsComponent.boards = data
    });
  }

  async onAdded(title: string) {
    (await this.boardService.addBoard(title))
    .subscribe( (data: Board) => {
      AllBoardsComponent.boards.push(data);
        this._snackBar.open(title + " added", "dismiss", {
          duration: 2000
        });
    });
  }

  async onDeleted(board: Board) {
    (await this.boardService.deleteBoard(board.id))
    .subscribe( () => {
      AllBoardsComponent.boards = AllBoardsComponent.boards.filter( (x: Board) => x.id !== board.id);
      this._snackBar.open(board.title + " deleted", "dismiss", {
        duration: 2000
      });
    });
  }
}
