import { Component } from '@angular/core';
import { AuthService } from '../services/auth.service';
import { Router } from '@angular/router';
import { TrainingRoutinesService } from '../services/training-routines.service';

/**
 * @component CustomTrainingRoutinesComponent
 * @description
 * The CustomTrainingRoutinesComponent manages the custom training routines of the user.
 * It handles displaying the user's custom routines, adding new routines, and deleting existing ones.
 * The component also checks for user authentication via a session token and redirects to the login page if necessary.
 *
 * @dependencies
 * - AuthService: Provides methods for session token retrieval, validation, and logout.
 * - TrainingRoutinesService: Provides methods for fetching, adding, and deleting custom training routines.
 * - Router: Facilitates navigation between application routes.
 *
 * @usage
 * This component is not standalone and uses external templates and styles:
 * - Template: './custom-training-routines.component.html'
 * - Styles: './custom-training-routines.component.css'
 */
@Component({
  selector: 'app-custom-training-routines',
  standalone: false,
  
  templateUrl: './custom-training-routines.component.html',
  styleUrl: './custom-training-routines.component.css'
})
export class CustomTrainingRoutinesComponent {
  errorMessage: string = '';
  successMessage: string = '';
  activeModal: string = '';

  userInfo: any = null;
  isLoggedIn: boolean = false;
  routines: any = [];
  isAdmin: boolean = false;

  newName: string = '';
  newDescription: string = '';
  newType: string = '';
  newRoutineLevel: string = '';
  optionsNeeds =
    ['Dumbbells', 'Barbell and weight plates', 'Adjustable workout bench',
      'Resistance bands', 'Kettlebells', 'Pull-up bar', 'Jump rope',
      'Treadmill or stationary bike', 'Swiss ball / stability ball'
  ];
  selectedNeeds: string[] = [];

  newNeeds: string = '';

  selectedRoutineId: number = 0;


  constructor(private authService: AuthService, private routinesService: TrainingRoutinesService, private router: Router) { }

  /**
 * @method ngOnInit
 * @description
 * Lifecycle hook that initializes the component. It validates the user session using the session token.
 * If the token is valid, it retrieves the user information and custom training routines.
 * If the token is missing or invalid, the user is redirected to the login page.
 */
  ngOnInit() {
    // Check if user is logged in by fetching the user info
    /* this.isLoggedIn = this.authService.isLoggedIn();  // A verificação da sessão agora é feita com o método isLoggedIn()
     if (this.isLoggedIn) {
       this.userInfo = this.authService.getUserInfo();  // Se estiver logado, pega as informações do usuário
     }
   }*/

    const token = this.authService.getSessionToken();

    if (token) {
      this.authService.validateSessionToken().subscribe(
        (response: any) => {
          this.isLoggedIn = true;
          this.userInfo = response.user;

          this.getRoutines();

          if (this.userInfo.type === 1) {
            this.isAdmin = true;
          } else {
            this.isAdmin = false;
          }
        },
        (error) => {
          this.authService.logout();
          this.router.navigate(['/login']);
        }
      );
    } else {
      //No token found, redirect to login
      this.router.navigate(['/login']);
    }

  }

  /**
 * @method getRoutines
 * @description
 * Fetches the custom training routines for the logged-in user using the TrainingRoutinesService.
 * On success, the routines array is updated; on failure, an error message is stored.
 */
  getRoutines() {
    this.routinesService.getCustomTrainingRoutines(this.userInfo.id).subscribe(
      (response: any) => {
        this.routines = response;
        console.log("Custom Routines loaded successfully!");
      },
      (error: any) => {
        this.errorMessage = error.error?.message; // Define a mensagem de erro se a requisição falhar
        console.log('Error loading Custom Routines:', error);
      }
    );
  }

  /**
 * @method addCustomRoutine
 * @description
 * Adds a new custom training routine by calling the TrainingRoutinesService.
 * Upon a successful response, the modal is closed and the user is redirected to edit the newly created routine.
 */
  addCustomRoutine() {
    console.log(this.userInfo.id);
    this.newNeeds = this.concatenatedString;
    if (this.newNeeds === '') this.newNeeds = 'Nothing';

    this.routinesService.addCustomRoutine(this.userInfo.id, this.newName, this.newDescription, this.newType, this.newRoutineLevel, this.newNeeds).subscribe(
      (response: any) => {
        console.log(response.message);
        console.log(response);
        this.closeModal();
        this.router.navigate([`/edit-custom-training-routine/${response.routineId}`]);
      },
      (error: any) => {
        console.log(error.error?.message);
      }
    );
  }

  /**
 * @method deleteCustomRoutine
 * @description
 * Deletes an existing custom training routine based on its identifier by calling the TrainingRoutinesService.
 * After deletion, it unselects the routine and refreshes the page.
 *
 * @param routineId - The unique identifier of the routine to be deleted.
 */
  deleteCustomRoutine(routineId: number) {
    this.routinesService.deleteRoutine(routineId).subscribe(
      (response: any) => {
        this.unselectRoutine();
        window.location.reload();
        console.log(response);
      },
      (error: any) => {
        this.unselectRoutine();
        console.log(error.error?.message);
      }
    )
  }

  /**
 * @method goToDashboard
 * @description Navigates the user to the dashboard page.
 */
  goToDashboard() {
    this.router.navigate(['/dashboard']);
  }

  /**
 * @method selectRoutine
 * @description Marks a routine as selected based on its identifier.
 *
 * @param routineId - The unique identifier of the routine to be selected.
 */
  selectRoutine(routineId: number) {
    this.selectedRoutineId = routineId;
  }

  /**
 * @method unselectRoutine
 * @description Clears the selection of a routine.
 */
  unselectRoutine() {
    this.selectedRoutineId = 0;
  }

  /**
  * @method openAddRoutineModal
  * @description Opens the modal dialog for adding a new custom training routine.
  */
  openAddRoutineModal(): void {
    this.activeModal = 'addRoutine';
  }

  /**
   * @method openDeleteRoutineModal
   * @description Opens the modal dialog for deleting a routine by first selecting the routine.
   *
   * @param routineId - The unique identifier of the routine to be deleted.
   */
  openDeleteRoutineModal(routineId: number): void {
    this.selectRoutine(routineId);
    this.activeModal = 'deleteRoutine';
  }

  /**
 * @method closeModal
 * @description Closes any open modal dialogs, resets the selected routine, and clears messages.
 */
  closeModal() {
    this.unselectRoutine();
    this.activeModal = ''; // Fechar a modal
    this.errorMessage = '';
    this.successMessage = '';
  }

  /**
 * @method logout
 * @description Logs out the current user by clearing the session and navigates to the homepage.
 */
  logout() {
    this.authService.logout();
    this.isLoggedIn = false;
    this.router.navigate(['/']);
  }

  /**
 * @getter concatenatedString
 * @description
 * Returns a single string composed of all selected equipment needs, joined by a comma and space.
 * This getter is dynamically updated based on the current state of the `selectedNeeds` array and is useful for
 * displaying or submitting the equipment list as a formatted string.
 */
  get concatenatedString(): string {
      return this.selectedNeeds.join(', ');
  }

  /**
 * @method onCheckboxChange
 * @description
 * Handles checkbox selection and updates the list of selected equipment needs (`selectedNeeds`) accordingly.
 * When a checkbox is checked, the corresponding option is added to the array; when unchecked, it is removed.
 * This method ensures that the `selectedNeeds` array always reflects the current state of the checkboxes.
 *
 * @param option - The equipment option associated with the checkbox that was interacted with.
 * @param event - The DOM event triggered by the checkbox change, used to determine the checked state.
 */
  onCheckboxChange(option: string, event: Event) {
    const isChecked = (event.target as HTMLInputElement).checked;

    if (isChecked) {
      this.selectedNeeds.push(option);
    } else {
      this.selectedNeeds = this.selectedNeeds.filter(o => o !== option);
    }
  }

}
